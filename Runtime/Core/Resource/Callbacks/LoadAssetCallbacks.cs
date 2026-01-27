//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 加载资源回调函数集。
    /// </summary>
    public sealed class LoadAssetCallbacks
    {
        private readonly LoadAssetSuccessCallback _loadAssetSuccessCallback;
        private readonly LoadAssetFailureCallback _loadAssetFailureCallback;

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback)
            : this(loadAssetSuccessCallback, null)
        {
        }

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        /// <param name="loadAssetFailureCallback">加载资源失败回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback, LoadAssetFailureCallback loadAssetFailureCallback)
        {
            if (loadAssetSuccessCallback == null)
            {
                throw new GameFrameworkException("Load asset success callback is invalid.");
            }

            _loadAssetSuccessCallback = loadAssetSuccessCallback;
            _loadAssetFailureCallback = loadAssetFailureCallback;
        }

        /// <summary>
        /// 获取加载资源成功回调函数。
        /// </summary>
        public LoadAssetSuccessCallback LoadAssetSuccessCallback
        {
            get
            {
                return _loadAssetSuccessCallback;
            }
        }

        /// <summary>
        /// 获取加载资源失败回调函数。
        /// </summary>
        public LoadAssetFailureCallback LoadAssetFailureCallback
        {
            get
            {
                return _loadAssetFailureCallback;
            }
        }
    }
}
