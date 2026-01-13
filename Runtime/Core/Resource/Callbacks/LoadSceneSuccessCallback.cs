//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 加载场景成功回调函数。
    /// </summary>
    /// <param name="sceneAssetAddress">要加载的场景资源地址。</param>
    /// <param name="sceneAsset">场景资源。</param>
    /// <param name="duration">加载持续时间。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadSceneSuccessCallback(AssetAddress sceneAssetAddress, object sceneAsset, float duration, object userData);
}
