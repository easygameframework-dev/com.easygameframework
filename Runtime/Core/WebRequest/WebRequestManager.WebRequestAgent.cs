//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.WebRequest
{
    internal sealed partial class WebRequestManager : GameFrameworkModule, IWebRequestManager
    {
        /// <summary>
        /// Web 请求代理。
        /// </summary>
        private sealed class WebRequestAgent : ITaskAgent<WebRequestTask>
        {
            private readonly IWebRequestAgentHelper _helper;
            private WebRequestTask _task;
            private float _waitTime;

            public GameFrameworkAction<WebRequestAgent> WebRequestAgentStart;
            public GameFrameworkAction<WebRequestAgent, byte[]> WebRequestAgentSuccess;
            public GameFrameworkAction<WebRequestAgent, string> WebRequestAgentFailure;

            /// <summary>
            /// 初始化 Web 请求代理的新实例。
            /// </summary>
            /// <param name="webRequestAgentHelper">Web 请求代理辅助器。</param>
            public WebRequestAgent(IWebRequestAgentHelper webRequestAgentHelper)
            {
                if (webRequestAgentHelper == null)
                {
                    throw new GameFrameworkException("Web request agent helper is invalid.");
                }

                _helper = webRequestAgentHelper;
                _task = null;
                _waitTime = 0f;

                WebRequestAgentStart = null;
                WebRequestAgentSuccess = null;
                WebRequestAgentFailure = null;
            }

            /// <summary>
            /// 获取 Web 请求任务。
            /// </summary>
            public WebRequestTask Task
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
            /// 初始化 Web 请求代理。
            /// </summary>
            public void Initialize()
            {
                _helper.WebRequestAgentHelperComplete += OnWebRequestAgentHelperComplete;
                _helper.WebRequestAgentHelperError += OnWebRequestAgentHelperError;
            }

            /// <summary>
            /// Web 请求代理轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (_task.Status == WebRequestTaskStatus.Doing)
                {
                    _waitTime += realElapseSeconds;
                    if (_waitTime >= _task.Timeout)
                    {
                        WebRequestAgentHelperErrorEventArgs webRequestAgentHelperErrorEventArgs = WebRequestAgentHelperErrorEventArgs.Create("Timeout");
                        OnWebRequestAgentHelperError(this, webRequestAgentHelperErrorEventArgs);
                        ReferencePool.Release(webRequestAgentHelperErrorEventArgs);
                    }
                }
            }

            /// <summary>
            /// 关闭并清理 Web 请求代理。
            /// </summary>
            public void Shutdown()
            {
                Reset();
                _helper.WebRequestAgentHelperComplete -= OnWebRequestAgentHelperComplete;
                _helper.WebRequestAgentHelperError -= OnWebRequestAgentHelperError;
            }

            /// <summary>
            /// 开始处理 Web 请求任务。
            /// </summary>
            /// <param name="task">要处理的 Web 请求任务。</param>
            /// <returns>开始处理任务的状态。</returns>
            public StartTaskStatus Start(WebRequestTask task)
            {
                if (task == null)
                {
                    throw new GameFrameworkException("Task is invalid.");
                }

                _task = task;
                _task.Status = WebRequestTaskStatus.Doing;

                if (WebRequestAgentStart != null)
                {
                    WebRequestAgentStart(this);
                }

                byte[] postData = _task.GetPostData();
                if (postData == null)
                {
                    _helper.Request(_task.WebRequestUri, _task.UserData);
                }
                else
                {
                    _helper.Request(_task.WebRequestUri, postData, _task.UserData);
                }

                _waitTime = 0f;
                return StartTaskStatus.CanResume;
            }

            /// <summary>
            /// 重置 Web 请求代理。
            /// </summary>
            public void Reset()
            {
                _helper.Reset();
                _task = null;
                _waitTime = 0f;
            }

            private void OnWebRequestAgentHelperComplete(object sender, WebRequestAgentHelperCompleteEventArgs e)
            {
                _helper.Reset();
                _task.Status = WebRequestTaskStatus.Done;

                if (WebRequestAgentSuccess != null)
                {
                    WebRequestAgentSuccess(this, e.GetWebResponseBytes());
                }

                _task.Done = true;
            }

            private void OnWebRequestAgentHelperError(object sender, WebRequestAgentHelperErrorEventArgs e)
            {
                _helper.Reset();
                _task.Status = WebRequestTaskStatus.Error;

                if (WebRequestAgentFailure != null)
                {
                    WebRequestAgentFailure(this, e.ErrorMessage);
                }

                _task.Done = true;
            }
        }
    }
}
