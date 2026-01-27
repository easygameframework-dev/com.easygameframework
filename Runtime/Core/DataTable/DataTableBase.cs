//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using EasyGameFramework.Core.Resource;

namespace EasyGameFramework.Core.DataTable
{
    /// <summary>
    /// 数据表基类。
    /// </summary>
    public abstract class DataTableBase : IDataProvider<DataTableBase>
    {
        private readonly string _name;
        private readonly DataProvider<DataTableBase> _dataProvider;

        /// <summary>
        /// 初始化数据表基类的新实例。
        /// </summary>
        public DataTableBase()
            : this(null)
        {
        }

        /// <summary>
        /// 初始化数据表基类的新实例。
        /// </summary>
        /// <param name="name">数据表名称。</param>
        public DataTableBase(string name)
        {
            _name = name ?? string.Empty;
            _dataProvider = new DataProvider<DataTableBase>(this);
        }

        /// <summary>
        /// 获取数据表名称。
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// 获取数据表完整名称。
        /// </summary>
        public string FullName
        {
            get
            {
                return new TypeNamePair(Type, _name).ToString();
            }
        }

        /// <summary>
        /// 获取数据表行的类型。
        /// </summary>
        public abstract Type Type
        {
            get;
        }

        /// <summary>
        /// 获取数据表行数。
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// 读取数据表成功事件。
        /// </summary>
        public event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess
        {
            add
            {
                _dataProvider.ReadDataSuccess += value;
            }
            remove
            {
                _dataProvider.ReadDataSuccess -= value;
            }
        }

        /// <summary>
        /// 读取数据表失败事件。
        /// </summary>
        public event EventHandler<ReadDataFailureEventArgs> ReadDataFailure
        {
            add
            {
                _dataProvider.ReadDataFailure += value;
            }
            remove
            {
                _dataProvider.ReadDataFailure -= value;
            }
        }

        /// <summary>
        /// 读取数据表。
        /// </summary>
        /// <param name="dataTableAssetAddress">数据表资源地址。</param>
        public void ReadData(AssetAddress dataTableAssetAddress)
        {
            _dataProvider.ReadData(dataTableAssetAddress);
        }

        /// <summary>
        /// 读取数据表。
        /// </summary>
        /// <param name="dataTableAssetAddress">数据表资源地址。</param>
        /// <param name="priority">加载数据表资源的优先级。</param>
        public void ReadData(AssetAddress dataTableAssetAddress, int priority)
        {
            _dataProvider.ReadData(dataTableAssetAddress, priority);
        }

        /// <summary>
        /// 读取数据表。
        /// </summary>
        /// <param name="dataTableAssetAddress">数据表资源地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void ReadData(AssetAddress dataTableAssetAddress, object userData)
        {
            _dataProvider.ReadData(dataTableAssetAddress, userData);
        }

        /// <summary>
        /// 读取数据表。
        /// </summary>
        /// <param name="dataTableAssetAddress">数据表资源地址。</param>
        /// <param name="priority">加载数据表资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void ReadData(AssetAddress dataTableAssetAddress, int priority, object userData)
        {
            _dataProvider.ReadData(dataTableAssetAddress, priority, userData);
        }

        /// <summary>
        /// 解析数据表。
        /// </summary>
        /// <param name="dataTableString">要解析的数据表字符串。</param>
        /// <returns>是否解析数据表成功。</returns>
        public bool ParseData(string dataTableString)
        {
            return _dataProvider.ParseData(dataTableString);
        }

        /// <summary>
        /// 解析数据表。
        /// </summary>
        /// <param name="dataTableString">要解析的数据表字符串。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析数据表成功。</returns>
        public bool ParseData(string dataTableString, object userData)
        {
            return _dataProvider.ParseData(dataTableString, userData);
        }

        /// <summary>
        /// 解析数据表。
        /// </summary>
        /// <param name="dataTableBytes">要解析的数据表二进制流。</param>
        /// <returns>是否解析数据表成功。</returns>
        public bool ParseData(byte[] dataTableBytes)
        {
            return _dataProvider.ParseData(dataTableBytes);
        }

        /// <summary>
        /// 解析数据表。
        /// </summary>
        /// <param name="dataTableBytes">要解析的数据表二进制流。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析数据表成功。</returns>
        public bool ParseData(byte[] dataTableBytes, object userData)
        {
            return _dataProvider.ParseData(dataTableBytes, userData);
        }

        /// <summary>
        /// 解析数据表。
        /// </summary>
        /// <param name="dataTableBytes">要解析的数据表二进制流。</param>
        /// <param name="startIndex">数据表二进制流的起始位置。</param>
        /// <param name="length">数据表二进制流的长度。</param>
        /// <returns>是否解析数据表成功。</returns>
        public bool ParseData(byte[] dataTableBytes, int startIndex, int length)
        {
            return _dataProvider.ParseData(dataTableBytes, startIndex, length);
        }

        /// <summary>
        /// 解析数据表。
        /// </summary>
        /// <param name="dataTableBytes">要解析的数据表二进制流。</param>
        /// <param name="startIndex">数据表二进制流的起始位置。</param>
        /// <param name="length">数据表二进制流的长度。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析数据表成功。</returns>
        public bool ParseData(byte[] dataTableBytes, int startIndex, int length, object userData)
        {
            return _dataProvider.ParseData(dataTableBytes, startIndex, length, userData);
        }

        /// <summary>
        /// 检查是否存在数据表行。
        /// </summary>
        /// <param name="id">数据表行的编号。</param>
        /// <returns>是否存在数据表行。</returns>
        public abstract bool HasDataRow(int id);

        /// <summary>
        /// 增加数据表行。
        /// </summary>
        /// <param name="dataRowString">要解析的数据表行字符串。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否增加数据表行成功。</returns>
        public abstract bool AddDataRow(string dataRowString, object userData);

        /// <summary>
        /// 增加数据表行。
        /// </summary>
        /// <param name="dataRowBytes">要解析的数据表行二进制流。</param>
        /// <param name="startIndex">数据表行二进制流的起始位置。</param>
        /// <param name="length">数据表行二进制流的长度。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否增加数据表行成功。</returns>
        public abstract bool AddDataRow(byte[] dataRowBytes, int startIndex, int length, object userData);

        /// <summary>
        /// 移除指定数据表行。
        /// </summary>
        /// <param name="id">要移除数据表行的编号。</param>
        /// <returns>是否移除数据表行成功。</returns>
        public abstract bool RemoveDataRow(int id);

        /// <summary>
        /// 清空所有数据表行。
        /// </summary>
        public abstract void RemoveAllDataRows();

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        internal void SetResourceManager(IResourceManager resourceManager)
        {
            _dataProvider.SetResourceManager(resourceManager);
        }

        /// <summary>
        /// 设置数据提供者辅助器。
        /// </summary>
        /// <param name="dataProviderHelper">数据提供者辅助器。</param>
        internal void SetDataProviderHelper(IDataProviderHelper<DataTableBase> dataProviderHelper)
        {
            _dataProvider.SetDataProviderHelper(dataProviderHelper);
        }

        /// <summary>
        /// 关闭并清理数据表。
        /// </summary>
        internal abstract void Shutdown();
    }
}
