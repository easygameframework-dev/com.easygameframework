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
    /// 加载场景失败事件。
    /// </summary>
    public sealed class LoadSceneFailureEventArgs : GameEventArgs
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
        /// <param name="e">内部事件。</param>
        /// <returns>创建的加载场景失败事件。</returns>
        public static LoadSceneFailureEventArgs Create(EasyGameFramework.Core.Scene.LoadSceneFailureEventArgs e)
        {
            LoadSceneFailureEventArgs loadSceneFailureEventArgs = ReferencePool.Acquire<LoadSceneFailureEventArgs>();
            loadSceneFailureEventArgs.SceneAssetAddress = e.SceneAssetAddress;
            loadSceneFailureEventArgs.ErrorMessage = e.ErrorMessage;
            loadSceneFailureEventArgs.UserData = e.UserData;
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
