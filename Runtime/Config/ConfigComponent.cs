//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Config;
using EasyGameFramework.Core.Resource;
using UnityEngine;

namespace EasyGameFramework
{
    /// <summary>
    /// 全局配置组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Config")]
    public sealed class ConfigComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private IConfigManager _configManager = null;
        private EventComponent _eventComponent = null;

        [SerializeField]
        private string _configHelperTypeName = "UnityGameFramework.Runtime.DefaultConfigHelper";

        [SerializeField]
        private ConfigHelperBase _customConfigHelper = null;

        [SerializeField]
        private int _cachedBytesSize = 0;

        /// <summary>
        /// 获取全局配置项数量。
        /// </summary>
        public int Count
        {
            get
            {
                return _configManager.Count;
            }
        }

        /// <summary>
        /// 获取缓冲二进制流的大小。
        /// </summary>
        public int CachedBytesSize
        {
            get
            {
                return _configManager.CachedBytesSize;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _configManager = GameFrameworkEntry.GetModule<IConfigManager>();
            if (_configManager == null)
            {
                Log.Fatal("Config manager is invalid.");
                return;
            }

            _configManager.ReadDataSuccess += OnReadDataSuccess;
            _configManager.ReadDataFailure += OnReadDataFailure;
        }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            _eventComponent = GameEntry.GetComponent<EventComponent>();
            if (_eventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            _configManager.SetResourceManager(GameFrameworkEntry.GetModule<IResourceManager>());

            ConfigHelperBase configHelper = Helper.CreateHelper(_configHelperTypeName, _customConfigHelper);
            if (configHelper == null)
            {
                Log.Error("Can not create config helper.");
                return;
            }

            configHelper.name = "Config Helper";
            Transform transform = configHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            _configManager.SetDataProviderHelper(configHelper);
            _configManager.SetConfigHelper(configHelper);
            if (_cachedBytesSize > 0)
            {
                EnsureCachedBytesSize(_cachedBytesSize);
            }
        }

        /// <summary>
        /// 确保二进制流缓存分配足够大小的内存并缓存。
        /// </summary>
        /// <param name="ensureSize">要确保二进制流缓存分配内存的大小。</param>
        public void EnsureCachedBytesSize(int ensureSize)
        {
            _configManager.EnsureCachedBytesSize(ensureSize);
        }

        /// <summary>
        /// 释放缓存的二进制流。
        /// </summary>
        public void FreeCachedBytes()
        {
            _configManager.FreeCachedBytes();
        }

        /// <summary>
        /// 读取全局配置。
        /// </summary>
        /// <param name="configAssetAddress">全局配置资源地址。</param>
        public void ReadData(AssetAddress configAssetAddress)
        {
            _configManager.ReadData(configAssetAddress);
        }

        /// <summary>
        /// 读取全局配置。
        /// </summary>
        /// <param name="configAssetAddress">全局配置资源地址。</param>
        /// <param name="priority">加载全局配置资源的优先级。</param>
        public void ReadData(AssetAddress configAssetAddress, int priority)
        {
            _configManager.ReadData(configAssetAddress, priority);
        }

        /// <summary>
        /// 读取全局配置。
        /// </summary>
        /// <param name="configAssetAddress">全局配置资源地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void ReadData(AssetAddress configAssetAddress, object userData)
        {
            _configManager.ReadData(configAssetAddress, userData);
        }

        /// <summary>
        /// 读取全局配置。
        /// </summary>
        /// <param name="configAssetAddress">全局配置资源地址。</param>
        /// <param name="priority">加载全局配置资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void ReadData(AssetAddress configAssetAddress, int priority, object userData)
        {
            _configManager.ReadData(configAssetAddress, priority, userData);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configString">要解析的全局配置字符串。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseData(string configString)
        {
            return _configManager.ParseData(configString);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configString">要解析的全局配置字符串。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseData(string configString, object userData)
        {
            return _configManager.ParseData(configString, userData);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configBytes">要解析的全局配置二进制流。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseData(byte[] configBytes)
        {
            return _configManager.ParseData(configBytes);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configBytes">要解析的全局配置二进制流。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseData(byte[] configBytes, object userData)
        {
            return _configManager.ParseData(configBytes, userData);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configBytes">要解析的全局配置二进制流。</param>
        /// <param name="startIndex">全局配置二进制流的起始位置。</param>
        /// <param name="length">全局配置二进制流的长度。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseData(byte[] configBytes, int startIndex, int length)
        {
            return _configManager.ParseData(configBytes, startIndex, length);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configBytes">要解析的全局配置二进制流。</param>
        /// <param name="startIndex">全局配置二进制流的起始位置。</param>
        /// <param name="length">全局配置二进制流的长度。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseData(byte[] configBytes, int startIndex, int length, object userData)
        {
            return _configManager.ParseData(configBytes, startIndex, length, userData);
        }

        /// <summary>
        /// 检查是否存在指定全局配置项。
        /// </summary>
        /// <param name="configName">要检查全局配置项的名称。</param>
        /// <returns>指定的全局配置项是否存在。</returns>
        public bool HasConfig(string configName)
        {
            return _configManager.HasConfig(configName);
        }

        /// <summary>
        /// 从指定全局配置项中读取布尔值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string configName)
        {
            return _configManager.GetBool(configName);
        }

        /// <summary>
        /// 从指定全局配置项中读取布尔值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string configName, bool defaultValue)
        {
            return _configManager.GetBool(configName, defaultValue);
        }

        /// <summary>
        /// 从指定全局配置项中读取整数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string configName)
        {
            return _configManager.GetInt(configName);
        }

        /// <summary>
        /// 从指定全局配置项中读取整数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string configName, int defaultValue)
        {
            return _configManager.GetInt(configName, defaultValue);
        }

        /// <summary>
        /// 从指定全局配置项中读取浮点数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string configName)
        {
            return _configManager.GetFloat(configName);
        }

        /// <summary>
        /// 从指定全局配置项中读取浮点数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string configName, float defaultValue)
        {
            return _configManager.GetFloat(configName, defaultValue);
        }

        /// <summary>
        /// 从指定全局配置项中读取字符串值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string configName)
        {
            return _configManager.GetString(configName);
        }

        /// <summary>
        /// 从指定全局配置项中读取字符串值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string configName, string defaultValue)
        {
            return _configManager.GetString(configName, defaultValue);
        }

        /// <summary>
        /// 增加指定全局配置项。
        /// </summary>
        /// <param name="configName">要增加全局配置项的名称。</param>
        /// <param name="boolValue">全局配置项布尔值。</param>
        /// <param name="intValue">全局配置项整数值。</param>
        /// <param name="floatValue">全局配置项浮点数值。</param>
        /// <param name="stringValue">全局配置项字符串值。</param>
        /// <returns>是否增加全局配置项成功。</returns>
        public bool AddConfig(string configName, bool boolValue, int intValue, float floatValue, string stringValue)
        {
            return _configManager.AddConfig(configName, boolValue, intValue, floatValue, stringValue);
        }

        /// <summary>
        /// 移除指定全局配置项。
        /// </summary>
        /// <param name="configName">要移除全局配置项的名称。</param>
        /// <returns>是否移除全局配置项成功。</returns>
        public bool RemoveConfig(string configName)
        {
            return _configManager.RemoveConfig(configName);
        }

        /// <summary>
        /// 清空所有全局配置项。
        /// </summary>
        public void RemoveAllConfigs()
        {
            _configManager.RemoveAllConfigs();
        }

        private void OnReadDataSuccess(object sender, ReadDataSuccessEventArgs e)
        {
            _eventComponent.Fire(this, LoadConfigSuccessEventArgs.Create(e));
        }

        private void OnReadDataFailure(object sender, ReadDataFailureEventArgs e)
        {
            Log.Warning("Load config failure, asset name '{0}', error message '{1}'.", e.DataAssetAddress.Location, e.ErrorMessage);
            _eventComponent.Fire(this, LoadConfigFailureEventArgs.Create(e));
        }
    }
}
