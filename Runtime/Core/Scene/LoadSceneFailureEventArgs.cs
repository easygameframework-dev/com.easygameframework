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
    /// 加载场景失败事件。
    /// </summary>
    public sealed class LoadSceneFailureEventArgs : GameFrameworkEventArgs
    {
        /// <summary>
        /// 初始化加载场景失败事件的新实例。
        /// </summary>
        public LoadSceneFailureEventArgs()
        {
            SceneAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
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
        /// 创建加载场景失败事件。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <param name="errorMessage">错误信息。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的加载场景失败事件。</returns>
        public static LoadSceneFailureEventArgs Create(AssetAddress sceneAssetAddress, string errorMessage, object userData)
        {
            LoadSceneFailureEventArgs loadSceneFailureEventArgs = ReferencePool.Acquire<LoadSceneFailureEventArgs>();
            loadSceneFailureEventArgs.SceneAssetAddress = sceneAssetAddress;
            loadSceneFailureEventArgs.ErrorMessage = errorMessage;
            loadSceneFailureEventArgs.UserData = userData;
            return loadSceneFailureEventArgs;
        }

        /// <summary>
        /// 清理加载场景失败事件。
        /// </summary>
        public override void Clear()
        {
            SceneAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
