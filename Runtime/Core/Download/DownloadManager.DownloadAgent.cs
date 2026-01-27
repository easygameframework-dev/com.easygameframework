//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.IO;

namespace EasyGameFramework.Core.Download
{
    internal sealed partial class DownloadManager : GameFrameworkModule, IDownloadManager
    {
        /// <summary>
        /// 下载代理。
        /// </summary>
        private sealed class DownloadAgent : ITaskAgent<DownloadTask>, IDisposable
        {
            private readonly IDownloadAgentHelper _helper;
            private DownloadTask _task;
            private FileStream _fileStream;
            private int _waitFlushSize;
            private float _waitTime;
            private long _startLength;
            private long _downloadedLength;
            private long _savedLength;
            private bool _disposed;

            public GameFrameworkAction<DownloadAgent> DownloadAgentStart;
            public GameFrameworkAction<DownloadAgent, int> DownloadAgentUpdate;
            public GameFrameworkAction<DownloadAgent, long> DownloadAgentSuccess;
            public GameFrameworkAction<DownloadAgent, string> DownloadAgentFailure;

            /// <summary>
            /// 初始化下载代理的新实例。
            /// </summary>
            /// <param name="downloadAgentHelper">下载代理辅助器。</param>
            public DownloadAgent(IDownloadAgentHelper downloadAgentHelper)
            {
                if (downloadAgentHelper == null)
                {
                    throw new GameFrameworkException("Download agent helper is invalid.");
                }

                _helper = downloadAgentHelper;
                _task = null;
                _fileStream = null;
                _waitFlushSize = 0;
                _waitTime = 0f;
                _startLength = 0L;
                _downloadedLength = 0L;
                _savedLength = 0L;
                _disposed = false;

                DownloadAgentStart = null;
                DownloadAgentUpdate = null;
                DownloadAgentSuccess = null;
                DownloadAgentFailure = null;
            }

            /// <summary>
            /// 获取下载任务。
            /// </summary>
            public DownloadTask Task
            {
                get
                {
                    return _task;
                }
            }

            /// <summary>
            /// 获取已经等待时间。
            /// </summary>
            public float WaitTime
            {
                get
                {
                    return _waitTime;
                }
            }

            /// <summary>
            /// 获取开始下载时已经存在的大小。
            /// </summary>
            public long StartLength
            {
                get
                {
                    return _startLength;
                }
            }

            /// <summary>
            /// 获取本次已经下载的大小。
            /// </summary>
            public long DownloadedLength
            {
                get
                {
                    return _downloadedLength;
                }
            }

            /// <summary>
            /// 获取当前的大小。
            /// </summary>
            public long CurrentLength
            {
                get
                {
                    return _startLength + _downloadedLength;
                }
            }

            /// <summary>
            /// 获取已经存盘的大小。
            /// </summary>
            public long SavedLength
            {
                get
                {
                    return _savedLength;
                }
            }

            /// <summary>
            /// 初始化下载代理。
            /// </summary>
            public void Initialize()
            {
                _helper.DownloadAgentHelperUpdateBytes += OnDownloadAgentHelperUpdateBytes;
                _helper.DownloadAgentHelperUpdateLength += OnDownloadAgentHelperUpdateLength;
                _helper.DownloadAgentHelperComplete += OnDownloadAgentHelperComplete;
                _helper.DownloadAgentHelperError += OnDownloadAgentHelperError;
            }

            /// <summary>
            /// 下载代理轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (_task.Status == DownloadTaskStatus.Doing)
                {
                    _waitTime += realElapseSeconds;
                    if (_waitTime >= _task.Timeout)
                    {
                        DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(false, "Timeout");
                        OnDownloadAgentHelperError(this, downloadAgentHelperErrorEventArgs);
                        ReferencePool.Release(downloadAgentHelperErrorEventArgs);
                    }
                }
            }

            /// <summary>
            /// 关闭并清理下载代理。
            /// </summary>
            public void Shutdown()
            {
                Dispose();

                _helper.DownloadAgentHelperUpdateBytes -= OnDownloadAgentHelperUpdateBytes;
                _helper.DownloadAgentHelperUpdateLength -= OnDownloadAgentHelperUpdateLength;
                _helper.DownloadAgentHelperComplete -= OnDownloadAgentHelperComplete;
                _helper.DownloadAgentHelperError -= OnDownloadAgentHelperError;
            }

            /// <summary>
            /// 开始处理下载任务。
            /// </summary>
            /// <param name="task">要处理的下载任务。</param>
            /// <returns>开始处理任务的状态。</returns>
            public StartTaskStatus Start(DownloadTask task)
            {
                if (task == null)
                {
                    throw new GameFrameworkException("Task is invalid.");
                }

                _task = task;

                _task.Status = DownloadTaskStatus.Doing;
                string downloadFile = Utility.Text.Format("{0}.download", _task.DownloadPath);

                try
                {
                    if (File.Exists(downloadFile))
                    {
                        _fileStream = File.OpenWrite(downloadFile);
                        _fileStream.Seek(0L, SeekOrigin.End);
                        _startLength = _savedLength = _fileStream.Length;
                        _downloadedLength = 0L;
                    }
                    else
                    {
                        string directory = Path.GetDirectoryName(_task.DownloadPath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        _fileStream = new FileStream(downloadFile, FileMode.Create, FileAccess.Write);
                        _startLength = _savedLength = _downloadedLength = 0L;
                    }

                    if (DownloadAgentStart != null)
                    {
                        DownloadAgentStart(this);
                    }

                    if (_startLength > 0L)
                    {
                        _helper.Download(_task.DownloadUri, _startLength, _task.UserData);
                    }
                    else
                    {
                        _helper.Download(_task.DownloadUri, _task.UserData);
                    }

                    return StartTaskStatus.CanResume;
                }
                catch (Exception exception)
                {
                    DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(false, exception.ToString());
                    OnDownloadAgentHelperError(this, downloadAgentHelperErrorEventArgs);
                    ReferencePool.Release(downloadAgentHelperErrorEventArgs);
                    return StartTaskStatus.UnknownError;
                }
            }

            /// <summary>
            /// 重置下载代理。
            /// </summary>
            public void Reset()
            {
                _helper.Reset();

                if (_fileStream != null)
                {
                    _fileStream.Close();
                    _fileStream = null;
                }

                _task = null;
                _waitFlushSize = 0;
                _waitTime = 0f;
                _startLength = 0L;
                _downloadedLength = 0L;
                _savedLength = 0L;
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
            private void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (_fileStream != null)
                    {
                        _fileStream.Dispose();
                        _fileStream = null;
                    }
                }

                _disposed = true;
            }

            private void OnDownloadAgentHelperUpdateBytes(object sender, DownloadAgentHelperUpdateBytesEventArgs e)
            {
                _waitTime = 0f;
                try
                {
                    _fileStream.Write(e.GetBytes(), e.Offset, e.Length);
                    _waitFlushSize += e.Length;
                    _savedLength += e.Length;

                    if (_waitFlushSize >= _task.FlushSize)
                    {
                        _fileStream.Flush();
                        _waitFlushSize = 0;
                    }
                }
                catch (Exception exception)
                {
                    DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(false, exception.ToString());
                    OnDownloadAgentHelperError(this, downloadAgentHelperErrorEventArgs);
                    ReferencePool.Release(downloadAgentHelperErrorEventArgs);
                }
            }

            private void OnDownloadAgentHelperUpdateLength(object sender, DownloadAgentHelperUpdateLengthEventArgs e)
            {
                _waitTime = 0f;
                _downloadedLength += e.DeltaLength;
                if (DownloadAgentUpdate != null)
                {
                    DownloadAgentUpdate(this, e.DeltaLength);
                }
            }

            private void OnDownloadAgentHelperComplete(object sender, DownloadAgentHelperCompleteEventArgs e)
            {
                _waitTime = 0f;
                _downloadedLength = e.Length;
                if (_savedLength != CurrentLength)
                {
                    throw new GameFrameworkException("Internal download error.");
                }

                _helper.Reset();
                _fileStream.Close();
                _fileStream = null;

                if (File.Exists(_task.DownloadPath))
                {
                    File.Delete(_task.DownloadPath);
                }

                File.Move(Utility.Text.Format("{0}.download", _task.DownloadPath), _task.DownloadPath);

                _task.Status = DownloadTaskStatus.Done;

                if (DownloadAgentSuccess != null)
                {
                    DownloadAgentSuccess(this, e.Length);
                }

                _task.Done = true;
            }

            private void OnDownloadAgentHelperError(object sender, DownloadAgentHelperErrorEventArgs e)
            {
                _helper.Reset();
                if (_fileStream != null)
                {
                    _fileStream.Close();
                    _fileStream = null;
                }

                if (e.DeleteDownloading)
                {
                    File.Delete(Utility.Text.Format("{0}.download", _task.DownloadPath));
                }

                _task.Status = DownloadTaskStatus.Error;

                if (DownloadAgentFailure != null)
                {
                    DownloadAgentFailure(this, e.ErrorMessage);
                }

                _task.Done = true;
            }
        }
    }
}
