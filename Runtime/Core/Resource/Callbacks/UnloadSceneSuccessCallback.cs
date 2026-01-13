//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 卸载场景成功回调函数。
    /// </summary>
    /// <param name="sceneAssetAddress">要卸载的场景资源地址。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void UnloadSceneSuccessCallback(AssetAddress sceneAssetAddress, object userData);
}
