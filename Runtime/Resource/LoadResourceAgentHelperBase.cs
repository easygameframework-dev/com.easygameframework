//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core.FileSystem;
using EasyGameFramework.Core.Resource;
using System;
using UnityEngine;

namespace EasyGameFramework
{
    /// <summary>
    /// 加载资源代理辅助器基类。
    /// </summary>
    public abstract class LoadResourceAgentHelperBase : MonoBehaviour, ILoadResourceAgentHelper
    {
        public abstract event EventHandler<LoadResourceAgentHelperLoadCompleteEventArgs> LoadComplete;
        public abstract event EventHandler<LoadResourceAgentHelperErrorEventArgs> Error;

        public abstract void LoadAsset(AssetAddress assetAddress, Type assetType, bool isScene, object userData);

        public abstract void Reset();
    }
}
