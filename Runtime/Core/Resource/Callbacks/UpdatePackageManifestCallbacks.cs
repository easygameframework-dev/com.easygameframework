namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 更新资源包清单成功时的回调函数。
    /// </summary>
    /// <param name="packageName">资源包名称。</param>
    public delegate void UpdatePackageManifestSuccessCallback(string packageName);

    /// <summary>
    /// 更新资源包清单失败时的回调函数。
    /// </summary>
    /// <param name="packageName">资源包名称。</param>
    /// <param name="errorMessage">错误信息。</param>
    public delegate void UpdatePackageManifestFailureCallback(string packageName, string errorMessage);

    /// <summary>
    /// 更新资源包清单时的回调函数集。
    /// </summary>
    public class UpdatePackageManifestCallbacks
    {
        private UpdatePackageManifestSuccessCallback _updatePackageManifestSuccessCallback;
        private UpdatePackageManifestFailureCallback _updatePackageManifestFailureCallback;

        /// <summary>
        /// 初始化更新资源包清单回调函数集的新实例。
        /// </summary>
        /// <param name="updatePackageManifestSuccessCallback">更新资源包清单成功时的回调函数。</param>
        /// <param name="updatePackageManifestFailureCallback">更新资源包清单失败时的回调函数。</param>
        public UpdatePackageManifestCallbacks(UpdatePackageManifestSuccessCallback updatePackageManifestSuccessCallback,
            UpdatePackageManifestFailureCallback updatePackageManifestFailureCallback)
        {
            _updatePackageManifestSuccessCallback = updatePackageManifestSuccessCallback;
            _updatePackageManifestFailureCallback = updatePackageManifestFailureCallback;
        }

        /// <summary>
        /// 获取更新资源包清单成功时的回调函数。
        /// </summary>
        public UpdatePackageManifestSuccessCallback UpdatePackageManifestSuccessCallback =>
            _updatePackageManifestSuccessCallback;

        /// <summary>
        /// 获取更新资源包清单失败时的回调函数。
        /// </summary>
        public UpdatePackageManifestFailureCallback UpdatePackageManifestFailureCallback =>
            _updatePackageManifestFailureCallback;
    }
}
