//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

#if !UNITY_2018_3_OR_NEWER

using GameFramework;
using GameFramework.WebRequest;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// WWW Web 请求代理辅助器。
    /// </summary>
    public class WWWWebRequestAgentHelper : WebRequestAgentHelperBase, IDisposable
    {
        private WWW _wWW = null;
        private bool _disposed = false;

        private EventHandler<WebRequestAgentHelperCompleteEventArgs> _webRequestAgentHelperCompleteEventHandler = null;
        private EventHandler<WebRequestAgentHelperErrorEventArgs> _webRequestAgentHelperErrorEventHandler = null;

        /// <summary>
        /// Web 请求代理辅助器完成事件。
        /// </summary>
        public override event EventHandler<WebRequestAgentHelperCompleteEventArgs> WebRequestAgentHelperComplete
        {
            add
            {
                _webRequestAgentHelperCompleteEventHandler += value;
            }
            remove
            {
                _webRequestAgentHelperCompleteEventHandler -= value;
            }
        }

        /// <summary>
        /// Web 请求代理辅助器错误事件。
        /// </summary>
        public override event EventHandler<WebRequestAgentHelperErrorEventArgs> WebRequestAgentHelperError
        {
            add
            {
                _webRequestAgentHelperErrorEventHandler += value;
            }
            remove
            {
                _webRequestAgentHelperErrorEventHandler -= value;
            }
        }

        /// <summary>
        /// 通过 Web 请求代理辅助器发送请求。
        /// </summary>
        /// <param name="webRequestUri">要发送的远程地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Request(string webRequestUri, object userData)
        {
            if (_webRequestAgentHelperCompleteEventHandler == null || _webRequestAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Web request agent helper handler is invalid.");
                return;
            }

            WWWFormInfo wwwFormInfo = (WWWFormInfo)userData;
            if (wwwFormInfo.WWWForm == null)
            {
                _wWW = new WWW(webRequestUri);
            }
            else
            {
                _wWW = new WWW(webRequestUri, wwwFormInfo.WWWForm);
            }
        }

        /// <summary>
        /// 通过 Web 请求代理辅助器发送请求。
        /// </summary>
        /// <param name="webRequestUri">要发送的远程地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Request(string webRequestUri, byte[] postData, object userData)
        {
            if (_webRequestAgentHelperCompleteEventHandler == null || _webRequestAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Web request agent helper handler is invalid.");
                return;
            }

            _wWW = new WWW(webRequestUri, postData);
        }

        /// <summary>
        /// 重置 Web 请求代理辅助器。
        /// </summary>
        public override void Reset()
        {
            if (_wWW != null)
            {
                _wWW.Dispose();
                _wWW = null;
            }
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="disposing">释放资源标记。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_wWW != null)
                {
                    _wWW.Dispose();
                    _wWW = null;
                }
            }

            _disposed = true;
        }

        private void Update()
        {
            if (_wWW == null || !_wWW.isDone)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_wWW.error))
            {
                WebRequestAgentHelperErrorEventArgs webRequestAgentHelperErrorEventArgs = WebRequestAgentHelperErrorEventArgs.Create(_wWW.error);
                _webRequestAgentHelperErrorEventHandler(this, webRequestAgentHelperErrorEventArgs);
                ReferencePool.Release(webRequestAgentHelperErrorEventArgs);
            }
            else
            {
                WebRequestAgentHelperCompleteEventArgs webRequestAgentHelperCompleteEventArgs = WebRequestAgentHelperCompleteEventArgs.Create(_wWW.bytes);
                _webRequestAgentHelperCompleteEventHandler(this, webRequestAgentHelperCompleteEventArgs);
                ReferencePool.Release(webRequestAgentHelperCompleteEventArgs);
            }
        }
    }
}

#endif
