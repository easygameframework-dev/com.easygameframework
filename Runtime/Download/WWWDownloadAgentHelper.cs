//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

#if !UNITY_2018_3_OR_NEWER

using GameFramework;
using GameFramework.Download;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// WWW 下载代理辅助器。
    /// </summary>
    public class WWWDownloadAgentHelper : DownloadAgentHelperBase, IDisposable
    {
        private WWW _wWW = null;
        private int _lastDownloadedSize = 0;
        private bool _disposed = false;

        private EventHandler<DownloadAgentHelperUpdateBytesEventArgs> _downloadAgentHelperUpdateBytesEventHandler = null;
        private EventHandler<DownloadAgentHelperUpdateLengthEventArgs> _downloadAgentHelperUpdateLengthEventHandler = null;
        private EventHandler<DownloadAgentHelperCompleteEventArgs> _downloadAgentHelperCompleteEventHandler = null;
        private EventHandler<DownloadAgentHelperErrorEventArgs> _downloadAgentHelperErrorEventHandler = null;

        /// <summary>
        /// 下载代理辅助器更新数据流事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperUpdateBytesEventArgs> DownloadAgentHelperUpdateBytes
        {
            add
            {
                _downloadAgentHelperUpdateBytesEventHandler += value;
            }
            remove
            {
                _downloadAgentHelperUpdateBytesEventHandler -= value;
            }
        }

        /// <summary>
        /// 下载代理辅助器更新数据大小事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength
        {
            add
            {
                _downloadAgentHelperUpdateLengthEventHandler += value;
            }
            remove
            {
                _downloadAgentHelperUpdateLengthEventHandler -= value;
            }
        }

        /// <summary>
        /// 下载代理辅助器完成事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete
        {
            add
            {
                _downloadAgentHelperCompleteEventHandler += value;
            }
            remove
            {
                _downloadAgentHelperCompleteEventHandler -= value;
            }
        }

        /// <summary>
        /// 下载代理辅助器错误事件。
        /// </summary>
        public override event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError
        {
            add
            {
                _downloadAgentHelperErrorEventHandler += value;
            }
            remove
            {
                _downloadAgentHelperErrorEventHandler -= value;
            }
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Download(string downloadUri, object userData)
        {
            if (_downloadAgentHelperUpdateBytesEventHandler == null || _downloadAgentHelperUpdateLengthEventHandler == null || _downloadAgentHelperCompleteEventHandler == null || _downloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            _wWW = new WWW(downloadUri);
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="fromPosition">下载数据起始位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Download(string downloadUri, long fromPosition, object userData)
        {
            if (_downloadAgentHelperUpdateBytesEventHandler == null || _downloadAgentHelperUpdateLengthEventHandler == null || _downloadAgentHelperCompleteEventHandler == null || _downloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "Range", Utility.Text.Format("bytes={0}-", fromPosition) }
            };

            _wWW = new WWW(downloadUri, null, header);
        }

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据。
        /// </summary>
        /// <param name="downloadUri">下载地址。</param>
        /// <param name="fromPosition">下载数据起始位置。</param>
        /// <param name="toPosition">下载数据结束位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Download(string downloadUri, long fromPosition, long toPosition, object userData)
        {
            if (_downloadAgentHelperUpdateBytesEventHandler == null || _downloadAgentHelperUpdateLengthEventHandler == null || _downloadAgentHelperCompleteEventHandler == null || _downloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "Range", Utility.Text.Format("bytes={0}-{1}", fromPosition, toPosition) }
            };

            _wWW = new WWW(downloadUri, null, header);
        }

        /// <summary>
        /// 重置下载代理辅助器。
        /// </summary>
        public override void Reset()
        {
            if (_wWW != null)
            {
                _wWW.Dispose();
                _wWW = null;
            }

            _lastDownloadedSize = 0;
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
            if (_wWW == null)
            {
                return;
            }

            int deltaLength = _wWW.bytesDownloaded - _lastDownloadedSize;
            if (deltaLength > 0)
            {
                _lastDownloadedSize = _wWW.bytesDownloaded;
                DownloadAgentHelperUpdateLengthEventArgs downloadAgentHelperUpdateLengthEventArgs = DownloadAgentHelperUpdateLengthEventArgs.Create(deltaLength);
                _downloadAgentHelperUpdateLengthEventHandler(this, downloadAgentHelperUpdateLengthEventArgs);
                ReferencePool.Release(downloadAgentHelperUpdateLengthEventArgs);
            }

            if (_wWW == null)
            {
                return;
            }

            if (!_wWW.isDone)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_wWW.error))
            {
                DownloadAgentHelperErrorEventArgs dodwnloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(_wWW.error.StartsWith(RangeNotSatisfiableErrorCode.ToString(), StringComparison.Ordinal), _wWW.error);
                _downloadAgentHelperErrorEventHandler(this, dodwnloadAgentHelperErrorEventArgs);
                ReferencePool.Release(dodwnloadAgentHelperErrorEventArgs);
            }
            else
            {
                byte[] bytes = _wWW.bytes;
                DownloadAgentHelperUpdateBytesEventArgs downloadAgentHelperUpdateBytesEventArgs = DownloadAgentHelperUpdateBytesEventArgs.Create(bytes, 0, bytes.Length);
                _downloadAgentHelperUpdateBytesEventHandler(this, downloadAgentHelperUpdateBytesEventArgs);
                ReferencePool.Release(downloadAgentHelperUpdateBytesEventArgs);

                DownloadAgentHelperCompleteEventArgs downloadAgentHelperCompleteEventArgs = DownloadAgentHelperCompleteEventArgs.Create(bytes.LongLength);
                _downloadAgentHelperCompleteEventHandler(this, downloadAgentHelperCompleteEventArgs);
                ReferencePool.Release(downloadAgentHelperCompleteEventArgs);
            }
        }
    }
}

#endif
