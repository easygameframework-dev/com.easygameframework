using System;

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 资源信息。
    /// </summary>
    public class AssetInfo
    {
        private readonly AssetAddress m_AssetAddress;
        private readonly Type m_AssetType;
        private readonly string m_Error;
        private readonly object m_UserData;

        /// <summary>
        /// 资源地址
        /// </summary>
        public AssetAddress Address => m_AssetAddress;

        /// <summary>
        /// 所属包裹
        /// </summary>
        public string PackageName => m_AssetAddress.PackageName;

        /// <summary>
        /// 资源类型
        /// </summary>
        public Type AssetType => m_AssetType;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error => m_Error;

        /// <summary>
        /// 资源名称
        /// </summary>
        public string AssetName => m_AssetAddress.Location;

        /// <summary>
        /// 引用对象
        /// </summary>
        public object UserData => m_UserData;

        /// <summary>
        /// 初始化资源信息的新实例。
        /// </summary>
        /// <param name="assetAddress">资源地址。</param>
        /// <param name="assetType">资源类型。</param>
        /// <param name="error">错误信息。</param>
        /// <param name="userData">用户自定义数据。</param>
        public AssetInfo(AssetAddress assetAddress, Type assetType, string error, object userData)
        {
            m_AssetAddress = assetAddress;
            m_AssetType = assetType;
            m_Error = error;
            m_UserData = userData;
        }
    }
}
