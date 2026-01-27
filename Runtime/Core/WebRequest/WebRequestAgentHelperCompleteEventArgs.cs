//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.WebRequest
{
    /// <summary>
    /// Web 请求代理辅助器完成事件。
    /// </summary>
    public sealed class WebRequestAgentHelperCompleteEventArgs : GameFrameworkEventArgs
    {
        private byte[] _webResponseBytes;

        /// <summary>
        /// 初始化 Web 请求代理辅助器完成事件的新实例。
        /// </summary>
        public WebRequestAgentHelperCompleteEventArgs()
        {
            _webResponseBytes = null;
        }

        /// <summary>
        /// 创建 Web 请求代理辅助器完成事件。
        /// </summary>
        /// <param name="webResponseBytes">Web 响应的数据流。</param>
        /// <returns>创建的 Web 请求代理辅助器完成事件。</returns>
        public static WebRequestAgentHelperCompleteEventArgs Create(byte[] webResponseBytes)
        {
            WebRequestAgentHelperCompleteEventArgs webRequestAgentHelperCompleteEventArgs = ReferencePool.Acquire<WebRequestAgentHelperCompleteEventArgs>();
            webRequestAgentHelperCompleteEventArgs._webResponseBytes = webResponseBytes;
            return webRequestAgentHelperCompleteEventArgs;
        }

        /// <summary>
        /// 清理 Web 请求代理辅助器完成事件。
        /// </summary>
        public override void Clear()
        {
            _webResponseBytes = null;
        }

        /// <summary>
        /// 获取 Web 响应的数据流。
        /// </summary>
        /// <returns>Web 响应的数据流。</returns>
        public byte[] GetWebResponseBytes()
        {
            return _webResponseBytes;
        }
    }
}
