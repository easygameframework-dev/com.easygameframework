//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core.Resource;

namespace EasyGameFramework.Core
{
    /// <summary>
    /// 读取数据失败事件。
    /// </summary>
    public sealed class ReadDataFailureEventArgs : GameFrameworkEventArgs
    {
        /// <summary>
        /// 初始化读取数据失败事件的新实例。
        /// </summary>
        public ReadDataFailureEventArgs()
        {
            DataAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
            UserData = null;
        }

        /// <summary>
        /// 获取数据资源地址。
        /// </summary>
        public AssetAddress DataAssetAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建读取数据失败事件。
        /// </summary>
        /// <param name="dataAssetAddress">数据资源地址。</param>
        /// <param name="errorMessage">错误信息。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的读取数据失败事件。</returns>
        public static ReadDataFailureEventArgs Create(AssetAddress dataAssetAddress, string errorMessage, object userData)
        {
            ReadDataFailureEventArgs loadDataFailureEventArgs = ReferencePool.Acquire<ReadDataFailureEventArgs>();
            loadDataFailureEventArgs.DataAssetAddress = dataAssetAddress;
            loadDataFailureEventArgs.ErrorMessage = errorMessage;
            loadDataFailureEventArgs.UserData = userData;
            return loadDataFailureEventArgs;
        }

        /// <summary>
        /// 清理读取数据失败事件。
        /// </summary>
        public override void Clear()
        {
            DataAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
