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
    /// 卸载场景成功事件。
    /// </summary>
    public sealed class UnloadSceneSuccessEventArgs : GameEventArgs
    {
        /// <summary>
        /// 初始化卸载场景成功事件的新实例。
        /// </summary>
        public UnloadSceneSuccessEventArgs()
        {
            SceneAssetAddress = AssetAddress.Empty;
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
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建卸载场景成功事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的卸载场景成功事件。</returns>
        public static UnloadSceneSuccessEventArgs Create(EasyGameFramework.Core.Scene.UnloadSceneSuccessEventArgs e)
        {
            UnloadSceneSuccessEventArgs unloadSceneSuccessEventArgs = ReferencePool.Acquire<UnloadSceneSuccessEventArgs>();
            unloadSceneSuccessEventArgs.SceneAssetAddress = e.SceneAssetAddress;
            unloadSceneSuccessEventArgs.UserData = e.UserData;
            return unloadSceneSuccessEventArgs;
        }

        /// <summary>
        /// 清理卸载场景成功事件。
        /// </summary>
        public override void Clear()
        {
            SceneAssetAddress = AssetAddress.Empty;
            UserData = null;
        }
    }
}
