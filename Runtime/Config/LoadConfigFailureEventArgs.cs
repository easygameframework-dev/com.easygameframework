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
    /// 加载全局配置失败事件。
    /// </summary>
    public sealed class LoadConfigFailureEventArgs : GameEventArgs
    {
        /// <summary>
        /// 初始化加载全局配置失败事件的新实例。
        /// </summary>
        public LoadConfigFailureEventArgs()
        {
            ConfigAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
            UserData = null;
        }

        /// <summary>
        /// 获取全局配置资源地址。
        /// </summary>
        public AssetAddress ConfigAssetAddress
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
        /// 创建加载全局配置失败事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的加载全局配置失败事件。</returns>
        public static LoadConfigFailureEventArgs Create(ReadDataFailureEventArgs e)
        {
            LoadConfigFailureEventArgs loadConfigFailureEventArgs = ReferencePool.Acquire<LoadConfigFailureEventArgs>();
            loadConfigFailureEventArgs.ConfigAssetAddress = e.DataAssetAddress;
            loadConfigFailureEventArgs.ErrorMessage = e.ErrorMessage;
            loadConfigFailureEventArgs.UserData = e.UserData;
            return loadConfigFailureEventArgs;
        }

        /// <summary>
        /// 清理加载全局配置失败事件。
        /// </summary>
        public override void Clear()
        {
            ConfigAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
