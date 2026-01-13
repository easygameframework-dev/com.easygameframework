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
    /// 加载场景成功事件。
    /// </summary>
    public sealed class LoadSceneSuccessEventArgs : GameEventArgs
    {
        /// <summary>
        /// 初始化加载场景成功事件的新实例。
        /// </summary>
        public LoadSceneSuccessEventArgs()
        {
            SceneAssetAddress = AssetAddress.Empty;
            SceneAsset = null;
            Duration = 0f;
            UserData = null;
        }

        /// <summary>
        /// 获取场景资源地址。
        /// </summary>
        public AssetAddress SceneAssetAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取场景资源。
        /// </summary>
        public object SceneAsset
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
        /// 创建加载场景成功事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的加载场景成功事件。</returns>
        public static LoadSceneSuccessEventArgs Create(EasyGameFramework.Core.Scene.LoadSceneSuccessEventArgs e)
        {
            LoadSceneSuccessEventArgs loadSceneSuccessEventArgs = ReferencePool.Acquire<LoadSceneSuccessEventArgs>();
            loadSceneSuccessEventArgs.SceneAssetAddress = e.SceneAssetAddress;
            loadSceneSuccessEventArgs.SceneAsset = e.SceneAsset;
            loadSceneSuccessEventArgs.Duration = e.Duration;
            loadSceneSuccessEventArgs.UserData = e.UserData;
            return loadSceneSuccessEventArgs;
        }

        /// <summary>
        /// 清理加载场景成功事件。
        /// </summary>
        public override void Clear()
        {
            SceneAssetAddress = AssetAddress.Empty;
            SceneAsset = null;
            Duration = 0f;
            UserData = null;
        }
    }
}
