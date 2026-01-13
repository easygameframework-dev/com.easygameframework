//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 卸载场景失败回调函数。
    /// </summary>
    /// <param name="sceneAssetAddress">要卸载的场景资源地址。</param>
    /// <param name="errorMessage">错误信息。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void UnloadSceneFailureCallback(AssetAddress sceneAssetAddress, string errorMessage, object userData);
}
