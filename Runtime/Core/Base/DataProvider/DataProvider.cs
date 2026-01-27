//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using EasyGameFramework.Core.Resource;

namespace EasyGameFramework.Core
{
    /// <summary>
    /// 数据提供者。
    /// </summary>
    /// <typeparam name="T">数据提供者的持有者的类型。</typeparam>
    internal sealed class DataProvider<T> : IDataProvider<T>
    {
        private const int BlockSize = 1024 * 4;
        private static byte[] s_cachedBytes = null;

        private readonly T _owner;
        private readonly LoadAssetCallbacks _loadAssetCallbacks;
        private IResourceManager _resourceManager;
        private IDataProviderHelper<T> _dataProviderHelper;
        private EventHandler<ReadDataSuccessEventArgs> _readDataSuccessEventHandler;
        private EventHandler<ReadDataFailureEventArgs> _readDataFailureEventHandler;

        /// <summary>
        /// 初始化数据提供者的新实例。
        /// </summary>
        /// <param name="owner">数据提供者的持有者。</param>
        public DataProvider(T owner)
        {
            _owner = owner;
            _loadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetOrBinaryFailureCallback);
            _resourceManager = null;
            _dataProviderHelper = null;
            _readDataSuccessEventHandler = null;
            _readDataFailureEventHandler = null;
        }

        /// <summary>
        /// 获取缓冲二进制流的大小。
        /// </summary>
        public static int CachedBytesSize
        {
            get
            {
                return s_cachedBytes != null ? s_cachedBytes.Length : 0;
            }
        }

        /// <summary>
        /// 读取数据成功事件。
        /// </summary>
        public event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess
        {
            add
            {
                _readDataSuccessEventHandler += value;
            }
            remove
            {
                _readDataSuccessEventHandler -= value;
            }
        }

        /// <summary>
        /// 读取数据失败事件。
        /// </summary>
        public event EventHandler<ReadDataFailureEventArgs> ReadDataFailure
        {
            add
            {
                _readDataFailureEventHandler += value;
            }
            remove
            {
                _readDataFailureEventHandler -= value;
            }
        }

        /// <summary>
        /// 确保二进制流缓存分配足够大小的内存并缓存。
        /// </summary>
        /// <param name="ensureSize">要确保二进制流缓存分配内存的大小。</param>
        public static void EnsureCachedBytesSize(int ensureSize)
        {
            if (ensureSize < 0)
            {
                throw new GameFrameworkException("Ensure size is invalid.");
            }

            if (s_cachedBytes == null || s_cachedBytes.Length < ensureSize)
            {
                FreeCachedBytes();
                int size = (ensureSize - 1 + BlockSize) / BlockSize * BlockSize;
                s_cachedBytes = new byte[size];
            }
        }

        /// <summary>
        /// 释放缓存的二进制流。
        /// </summary>
        public static void FreeCachedBytes()
        {
            s_cachedBytes = null;
        }

        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataAssetAddress">内容资源地址。</param>
        public void ReadData(AssetAddress dataAssetAddress)
        {
            ReadData(dataAssetAddress, Constant.DefaultPriority, null);
        }

        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataAssetAddress">内容资源地址。</param>
        /// <param name="priority">加载数据资源的优先级。</param>
        public void ReadData(AssetAddress dataAssetAddress, int priority)
        {
            ReadData(dataAssetAddress, priority, null);
        }

        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataAssetAddress">内容资源地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void ReadData(AssetAddress dataAssetAddress, object userData)
        {
            ReadData(dataAssetAddress, Constant.DefaultPriority, userData);
        }

        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataAssetAddress">内容资源地址。</param>
        /// <param name="priority">加载数据资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void ReadData(AssetAddress dataAssetAddress, int priority, object userData)
        {
            if (_resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (_dataProviderHelper == null)
            {
                throw new GameFrameworkException("You must set data provider helper first.");
            }

            HasAssetResult result = _resourceManager.HasAsset(dataAssetAddress);
            switch (result)
            {
                case HasAssetResult.AssetOnDisk:
                case HasAssetResult.AssetOnFileSystem:
                    _resourceManager.LoadAsset(dataAssetAddress, _loadAssetCallbacks, null, priority, userData);
                    break;
                default:
                    throw new GameFrameworkException(Utility.Text.Format("Data asset '{0}' is '{1}'.", dataAssetAddress, result));
            }
        }

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataString">要解析的内容字符串。</param>
        /// <returns>是否解析内容成功。</returns>
        public bool ParseData(string dataString)
        {
            return ParseData(dataString, null);
        }

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataString">要解析的内容字符串。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析内容成功。</returns>
        public bool ParseData(string dataString, object userData)
        {
            if (_dataProviderHelper == null)
            {
                throw new GameFrameworkException("You must set data helper first.");
            }

            if (dataString == null)
            {
                throw new GameFrameworkException("Data string is invalid.");
            }

            try
            {
                return _dataProviderHelper.ParseData(_owner, dataString, userData);
            }
            catch (Exception exception)
            {
                if (exception is GameFrameworkException)
                {
                    throw;
                }

                throw new GameFrameworkException(Utility.Text.Format("Can not parse data string with exception '{0}'.", exception), exception);
            }
        }

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <returns>是否解析内容成功。</returns>
        public bool ParseData(byte[] dataBytes)
        {
            if (dataBytes == null)
            {
                throw new GameFrameworkException("Data bytes is invalid.");
            }

            return ParseData(dataBytes, 0, dataBytes.Length, null);
        }

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析内容成功。</returns>
        public bool ParseData(byte[] dataBytes, object userData)
        {
            if (dataBytes == null)
            {
                throw new GameFrameworkException("Data bytes is invalid.");
            }

            return ParseData(dataBytes, 0, dataBytes.Length, userData);
        }

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <param name="startIndex">内容二进制流的起始位置。</param>
        /// <param name="length">内容二进制流的长度。</param>
        /// <returns>是否解析内容成功。</returns>
        public bool ParseData(byte[] dataBytes, int startIndex, int length)
        {
            return ParseData(dataBytes, startIndex, length, null);
        }

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <param name="startIndex">内容二进制流的起始位置。</param>
        /// <param name="length">内容二进制流的长度。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析内容成功。</returns>
        public bool ParseData(byte[] dataBytes, int startIndex, int length, object userData)
        {
            if (_dataProviderHelper == null)
            {
                throw new GameFrameworkException("You must set data helper first.");
            }

            if (dataBytes == null)
            {
                throw new GameFrameworkException("Data bytes is invalid.");
            }

            if (startIndex < 0 || length < 0 || startIndex + length > dataBytes.Length)
            {
                throw new GameFrameworkException("Start index or length is invalid.");
            }

            try
            {
                return _dataProviderHelper.ParseData(_owner, dataBytes, startIndex, length, userData);
            }
            catch (Exception exception)
            {
                if (exception is GameFrameworkException)
                {
                    throw;
                }

                throw new GameFrameworkException(Utility.Text.Format("Can not parse data bytes with exception '{0}'.", exception), exception);
            }
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        internal void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            _resourceManager = resourceManager;
        }

        /// <summary>
        /// 设置数据提供者辅助器。
        /// </summary>
        /// <param name="dataProviderHelper">数据提供者辅助器。</param>
        internal void SetDataProviderHelper(IDataProviderHelper<T> dataProviderHelper)
        {
            if (dataProviderHelper == null)
            {
                throw new GameFrameworkException("Data provider helper is invalid.");
            }

            _dataProviderHelper = dataProviderHelper;
        }

        private void LoadAssetSuccessCallback(AssetAddress assetAddress, object dataAsset, float duration, object userData)
        {
            try
            {
                if (!_dataProviderHelper.ReadData(_owner, assetAddress, dataAsset, userData))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Load data failure in data provider helper, data asset name '{0}'.", assetAddress));
                }

                if (_readDataSuccessEventHandler != null)
                {
                    ReadDataSuccessEventArgs loadDataSuccessEventArgs = ReadDataSuccessEventArgs.Create(assetAddress, duration, userData);
                    _readDataSuccessEventHandler(this, loadDataSuccessEventArgs);
                    ReferencePool.Release(loadDataSuccessEventArgs);
                }
            }
            catch (Exception exception)
            {
                if (_readDataFailureEventHandler != null)
                {
                    ReadDataFailureEventArgs loadDataFailureEventArgs = ReadDataFailureEventArgs.Create(assetAddress, exception.ToString(), userData);
                    _readDataFailureEventHandler(this, loadDataFailureEventArgs);
                    ReferencePool.Release(loadDataFailureEventArgs);
                    return;
                }

                throw;
            }
            finally
            {
                _dataProviderHelper.ReleaseDataAsset(_owner, dataAsset);
            }
        }

        private void LoadAssetOrBinaryFailureCallback(AssetAddress assetAddress, LoadResourceStatus status, string errorMessage, object userData)
        {
            string appendErrorMessage = Utility.Text.Format("Load data failure, data asset name '{0}', status '{1}', error message '{2}'.", assetAddress, status, errorMessage);
            if (_readDataFailureEventHandler != null)
            {
                ReadDataFailureEventArgs loadDataFailureEventArgs = ReadDataFailureEventArgs.Create(assetAddress, appendErrorMessage, userData);
                _readDataFailureEventHandler(this, loadDataFailureEventArgs);
                ReferencePool.Release(loadDataFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(appendErrorMessage);
        }
    }
}
