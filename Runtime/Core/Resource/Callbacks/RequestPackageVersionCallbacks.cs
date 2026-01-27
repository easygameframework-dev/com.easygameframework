namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 请求资源包版本成功时的回调函数。
    /// </summary>
    /// <param name="packageName">资源包名称。</param>
    /// <param name="packageVersion">资源包版本。</param>
    public delegate void RequestPackageVersionSuccessCallback(string packageName, string packageVersion);

    /// <summary>
    /// 请求资源包版本失败时的回调函数。
    /// </summary>
    /// <param name="packageName">资源包名称。</param>
    /// <param name="errorMessage">错误信息。</param>
    public delegate void RequestPackageVersionFailureCallback(string packageName, string errorMessage);

    /// <summary>
    /// 请求资源包版本时的回调函数集。
    /// </summary>
    public class RequestPackageVersionCallbacks
    {
        private RequestPackageVersionSuccessCallback _requestPackageVersionSuccessCallback;
        private RequestPackageVersionFailureCallback _requestPackageVersionFailureCallback;

        /// <summary>
        /// 初始化请求资源包版本回调函数集的新实例。
        /// </summary>
        /// <param name="requestPackageVersionSuccessCallback">请求资源包版本成功时的回调函数。</param>
        /// <param name="requestPackageVersionFailureCallback">请求资源包版本失败时的回调函数。</param>
        public RequestPackageVersionCallbacks(RequestPackageVersionSuccessCallback requestPackageVersionSuccessCallback,
            RequestPackageVersionFailureCallback requestPackageVersionFailureCallback)
        {
            _requestPackageVersionSuccessCallback = requestPackageVersionSuccessCallback;
            _requestPackageVersionFailureCallback = requestPackageVersionFailureCallback;
        }

        /// <summary>
        /// 获取请求资源包版本成功时的回调函数。
        /// </summary>
        public RequestPackageVersionSuccessCallback RequestPackageVersionSuccessCallback => _requestPackageVersionSuccessCallback;

        /// <summary>
        /// 获取请求资源包版本失败时的回调函数。
        /// </summary>
        public RequestPackageVersionFailureCallback RequestPackageVersionFailureCallback => _requestPackageVersionFailureCallback;
    }
}
