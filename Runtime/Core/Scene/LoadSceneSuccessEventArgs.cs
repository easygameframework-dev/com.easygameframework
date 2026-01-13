//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core.Resource;

namespace EasyGameFramework.Core.Scene
{
    /// <summary>
    /// 加载场景成功事件。
    /// </summary>
    public sealed class LoadSceneSuccessEventArgs : GameFrameworkEventArgs
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
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <param name="sceneAsset">场景资源。</param>
        /// <param name="duration">加载持续时间。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的加载场景成功事件。</returns>
        public static LoadSceneSuccessEventArgs Create(AssetAddress sceneAssetAddress, object sceneAsset, float duration, object userData)
        {
            LoadSceneSuccessEventArgs loadSceneSuccessEventArgs = ReferencePool.Acquire<LoadSceneSuccessEventArgs>();
            loadSceneSuccessEventArgs.SceneAssetAddress = sceneAssetAddress;
            loadSceneSuccessEventArgs.SceneAsset = sceneAsset;
            loadSceneSuccessEventArgs.Duration = duration;
            loadSceneSuccessEventArgs.UserData = userData;
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
