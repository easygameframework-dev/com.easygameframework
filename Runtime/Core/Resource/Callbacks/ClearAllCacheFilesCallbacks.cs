namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 清理所有缓存文件完成时的回调函数。
    /// </summary>
    /// <param name="hasError">是否有错误发生。</param>
    public delegate void ClearAllCacheFilesCompleteCallback(bool hasError);

    /// <summary>
    /// 清理所有缓存文件时的回调函数集。
    /// </summary>
    public class ClearAllCacheFilesCallbacks
    {
        private ClearAllCacheFilesCompleteCallback _clearAllCacheFilesComplete;
        private ClearPackageCacheFilesSuccessCallback _clearPackageCacheFilesSuccess;
        private ClearPackageCacheFilesFailureCallback _clearPackageCacheFilesFailure;

        /// <summary>
        /// 初始化清理所有缓存文件回调函数集的新实例。
        /// </summary>
        /// <param name="clearAllCacheFilesComplete">清理所有缓存文件完成时的回调函数。</param>
        /// <param name="clearPackageCacheFilesSuccess">清理资源包缓存文件成功时的回调函数。</param>
        /// <param name="clearPackageCacheFilesFailure">清理资源包缓存文件失败时的回调函数。</param>
        public ClearAllCacheFilesCallbacks(
            ClearAllCacheFilesCompleteCallback clearAllCacheFilesComplete,
            ClearPackageCacheFilesSuccessCallback clearPackageCacheFilesSuccess,
            ClearPackageCacheFilesFailureCallback clearPackageCacheFilesFailure)
        {
            _clearAllCacheFilesComplete = clearAllCacheFilesComplete;
            _clearPackageCacheFilesSuccess = clearPackageCacheFilesSuccess;
            _clearPackageCacheFilesFailure = clearPackageCacheFilesFailure;
        }

        /// <summary>
        /// 获取清理所有缓存文件完成时的回调函数。
        /// </summary>
        public ClearAllCacheFilesCompleteCallback ClearAllCacheFilesComplete =>
            _clearAllCacheFilesComplete;

        /// <summary>
        /// 获取清理资源包缓存文件成功时的回调函数。
        /// </summary>
        public ClearPackageCacheFilesSuccessCallback ClearPackageCacheFilesSuccess =>
            _clearPackageCacheFilesSuccess;

        /// <summary>
        /// 获取清理资源包缓存文件失败时的回调函数。
        /// </summary>
        public ClearPackageCacheFilesFailureCallback ClearPackageCacheFilesFailure =>
            _clearPackageCacheFilesFailure;
    }
}
