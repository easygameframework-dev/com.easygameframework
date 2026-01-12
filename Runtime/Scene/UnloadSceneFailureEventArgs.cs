//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Event;

namespace EasyGameFramework
{
    /// <summary>
    /// 卸载场景失败事件。
    /// </summary>
    public sealed class UnloadSceneFailureEventArgs : GameEventArgs
    {
        /// <summary>
        /// 初始化卸载场景失败事件的新实例。
        /// </summary>
        public UnloadSceneFailureEventArgs()
        {
            PackageName = null;
            SceneAssetName = null;
            UserData = null;
        }

        /// <summary>
        /// 获取资源包名称。
        /// </summary>
        public string PackageName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取场景资源名称。
        /// </summary>
        public string SceneAssetName
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
        /// 创建卸载场景失败事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的卸载场景失败事件。</returns>
        public static UnloadSceneFailureEventArgs Create(EasyGameFramework.Core.Scene.UnloadSceneFailureEventArgs e)
        {
            UnloadSceneFailureEventArgs unloadSceneFailureEventArgs = ReferencePool.Acquire<UnloadSceneFailureEventArgs>();
            unloadSceneFailureEventArgs.PackageName = e.PackageName;
            unloadSceneFailureEventArgs.SceneAssetName = e.SceneAssetName;
            unloadSceneFailureEventArgs.ErrorMessage = e.ErrorMessage;
            unloadSceneFailureEventArgs.UserData = e.UserData;
            return unloadSceneFailureEventArgs;
        }

        /// <summary>
        /// 清理卸载场景失败事件。
        /// </summary>
        public override void Clear()
        {
            PackageName = null;
            SceneAssetName = null;
            UserData = null;
        }
    }
}
