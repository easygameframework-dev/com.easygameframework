namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 初始化资源包成功时的回调函数。
    /// </summary>
    /// <param name="packageName">资源包名称。</param>
    public delegate void InitPackageSuccessCallback(string packageName);

    /// <summary>
    /// 初始化资源包失败时的回调函数。
    /// </summary>
    /// <param name="packageName">资源包名称。</param>
    /// <param name="errorMessage">错误信息。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void InitPackageFailureCallback(string packageName, string errorMessage, object userData);

    /// <summary>
    /// 初始化资源包时的回调函数集。
    /// </summary>
    public class InitPackageCallbacks
    {
        private readonly InitPackageSuccessCallback _initPackageSuccess;
        private readonly InitPackageFailureCallback _initPackageFailure;

        /// <summary>
        /// 初始化资源包回调函数集的新实例。
        /// </summary>
        /// <param name="initPackageSuccessCallback">初始化资源包成功时的回调函数。</param>
        /// <param name="initPackageFailureCallback">初始化资源包失败时的回调函数。</param>
        public InitPackageCallbacks(InitPackageSuccessCallback initPackageSuccessCallback,
            InitPackageFailureCallback initPackageFailureCallback)
        {
            _initPackageSuccess = initPackageSuccessCallback;
            _initPackageFailure = initPackageFailureCallback;
        }

        /// <summary>
        /// 获取初始化资源包成功时的回调函数。
        /// </summary>
        public InitPackageSuccessCallback InitPackageSuccess => _initPackageSuccess;

        /// <summary>
        /// 获取初始化资源包失败时的回调函数。
        /// </summary>
        public InitPackageFailureCallback InitPackageFailure => _initPackageFailure;
    }

}
