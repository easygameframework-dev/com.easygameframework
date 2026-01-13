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
    /// 加载全局配置成功事件。
    /// </summary>
    public sealed class LoadConfigSuccessEventArgs : GameEventArgs
    {
        /// <summary>
        /// 初始化加载全局配置成功事件编号的新实例。
        /// </summary>
        public LoadConfigSuccessEventArgs()
        {
            ConfigAssetAddress = AssetAddress.Empty;
            Duration = 0f;
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
        /// 获取加载持续时间。
        /// </summary>
        public float Duration
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
        /// 创建加载全局配置成功事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的加载全局配置成功事件。</returns>
        public static LoadConfigSuccessEventArgs Create(ReadDataSuccessEventArgs e)
        {
            LoadConfigSuccessEventArgs loadConfigSuccessEventArgs = ReferencePool.Acquire<LoadConfigSuccessEventArgs>();
            loadConfigSuccessEventArgs.ConfigAssetAddress = e.DataAssetAddress;
            loadConfigSuccessEventArgs.Duration = e.Duration;
            loadConfigSuccessEventArgs.UserData = e.UserData;
            return loadConfigSuccessEventArgs;
        }

        /// <summary>
        /// 清理加载全局配置成功事件。
        /// </summary>
        public override void Clear()
        {
            ConfigAssetAddress = AssetAddress.Empty;
            Duration = 0f;
            UserData = null;
        }
    }
}
