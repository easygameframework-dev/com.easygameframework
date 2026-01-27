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
    /// Web 请求成功事件。
    /// </summary>
    public sealed class WebRequestSuccessEventArgs : GameEventArgs
    {
        private byte[] _webResponseBytes = null;

        /// <summary>
        /// 初始化 Web 请求成功事件的新实例。
        /// </summary>
        public WebRequestSuccessEventArgs()
        {
            SerialId = 0;
            WebRequestUri = null;
            _webResponseBytes = null;
            UserData = null;
        }

        /// <summary>
        /// 获取 Web 请求任务的序列编号。
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取 Web 请求地址。
        /// </summary>
        public string WebRequestUri
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
        /// 获取 Web 响应的数据流。
        /// </summary>
        /// <returns>Web 响应的数据流。</returns>
        public byte[] GetWebResponseBytes()
        {
            return _webResponseBytes;
        }

        /// <summary>
        /// 创建 Web 请求成功事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的 Web 请求成功事件。</returns>
        public static WebRequestSuccessEventArgs Create(EasyGameFramework.Core.WebRequest.WebRequestSuccessEventArgs e)
        {
            WWWFormInfo wwwFormInfo = (WWWFormInfo)e.UserData;
            WebRequestSuccessEventArgs webRequestSuccessEventArgs = ReferencePool.Acquire<WebRequestSuccessEventArgs>();
            webRequestSuccessEventArgs.SerialId = e.SerialId;
            webRequestSuccessEventArgs.WebRequestUri = e.WebRequestUri;
            webRequestSuccessEventArgs._webResponseBytes = e.GetWebResponseBytes();
            webRequestSuccessEventArgs.UserData = wwwFormInfo.UserData;
            ReferencePool.Release(wwwFormInfo);
            return webRequestSuccessEventArgs;
        }

        /// <summary>
        /// 清理 Web 请求成功事件。
        /// </summary>
        public override void Clear()
        {
            SerialId = 0;
            WebRequestUri = null;
            _webResponseBytes = null;
            UserData = null;
        }
    }
}
