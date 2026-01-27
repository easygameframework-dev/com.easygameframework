using System;

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 资源信息。
    /// </summary>
    public class AssetInfo
    {
        private readonly AssetAddress _assetAddress;
        private readonly Type _assetType;
        private readonly string _error;
        private readonly object _userData;

        /// <summary>
        /// 资源地址
        /// </summary>
        public AssetAddress Address => _assetAddress;

        /// <summary>
        /// 所属包裹
        /// </summary>
        public string PackageName => _assetAddress.PackageName;

        /// <summary>
        /// 资源类型
        /// </summary>
        public Type AssetType => _assetType;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error => _error;

        /// <summary>
        /// 资源名称
        /// </summary>
        public string AssetName => _assetAddress.Location;

        /// <summary>
        /// 引用对象
        /// </summary>
        public object UserData => _userData;

        /// <summary>
        /// 初始化资源信息的新实例。
        /// </summary>
        /// <param name="assetAddress">资源地址。</param>
        /// <param name="assetType">资源类型。</param>
        /// <param name="error">错误信息。</param>
        /// <param name="userData">用户自定义数据。</param>
        public AssetInfo(AssetAddress assetAddress, Type assetType, string error, object userData)
        {
            _assetAddress = assetAddress;
            _assetType = assetType;
            _error = error;
            _userData = userData;
        }
    }
}
