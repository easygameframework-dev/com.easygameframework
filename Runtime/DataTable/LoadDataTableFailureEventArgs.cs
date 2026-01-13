//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Event;
using EasyGameFramework.Core.Resource;

namespace EasyGameFramework
{
    /// <summary>
    /// 加载数据表失败事件。
    /// </summary>
    public sealed class LoadDataTableFailureEventArgs : GameEventArgs
    {
        /// <summary>
        /// 初始化加载数据表失败事件的新实例。
        /// </summary>
        public LoadDataTableFailureEventArgs()
        {
            DataTableAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
            UserData = null;
        }

        /// <summary>
        /// 获取数据表资源地址。
        /// </summary>
        public AssetAddress DataTableAssetAddress
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
        /// 创建加载数据表失败事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的加载数据表失败事件。</returns>
        public static LoadDataTableFailureEventArgs Create(ReadDataFailureEventArgs e)
        {
            LoadDataTableFailureEventArgs loadDataTableFailureEventArgs = ReferencePool.Acquire<LoadDataTableFailureEventArgs>();
            loadDataTableFailureEventArgs.DataTableAssetAddress = e.DataAssetAddress;
            loadDataTableFailureEventArgs.ErrorMessage = e.ErrorMessage;
            loadDataTableFailureEventArgs.UserData = e.UserData;
            return loadDataTableFailureEventArgs;
        }

        /// <summary>
        /// 清理加载数据表失败事件。
        /// </summary>
        public override void Clear()
        {
            DataTableAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
