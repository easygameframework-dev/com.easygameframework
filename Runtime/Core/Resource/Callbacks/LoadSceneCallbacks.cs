//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 加载场景回调函数集。
    /// </summary>
    public sealed class LoadSceneCallbacks
    {
        private readonly LoadSceneSuccessCallback _loadSceneSuccessCallback;
        private readonly LoadSceneFailureCallback _loadSceneFailureCallback;

        /// <summary>
        /// 初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneFailureCallback">加载场景失败回调函数。</param>
        public LoadSceneCallbacks(LoadSceneSuccessCallback loadSceneSuccessCallback, LoadSceneFailureCallback loadSceneFailureCallback = null)
        {
            if (loadSceneSuccessCallback == null)
            {
                throw new GameFrameworkException("Load scene success callback is invalid.");
            }

            _loadSceneSuccessCallback = loadSceneSuccessCallback;
            _loadSceneFailureCallback = loadSceneFailureCallback;
        }

        /// <summary>
        /// 获取加载场景成功回调函数。
        /// </summary>
        public LoadSceneSuccessCallback LoadSceneSuccessCallback
        {
            get
            {
                return _loadSceneSuccessCallback;
            }
        }

        /// <summary>
        /// 获取加载场景失败回调函数。
        /// </summary>
        public LoadSceneFailureCallback LoadSceneFailureCallback
        {
            get
            {
                return _loadSceneFailureCallback;
            }
        }
    }
}
