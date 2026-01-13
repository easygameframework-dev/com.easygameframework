//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Event;
using EasyGameFramework.Core.Resource;

namespace EasyGameFramework
{
    /// <summary>
    /// 加载字典失败事件。
    /// </summary>
    public sealed class LoadDictionaryFailureEventArgs : GameEventArgs
    {
        /// <summary>
        /// 初始化加载字典失败事件的新实例。
        /// </summary>
        public LoadDictionaryFailureEventArgs()
        {
            DictionaryAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
            UserData = null;
        }

        /// <summary>
        /// 获取字典资源地址。
        /// </summary>
        public AssetAddress DictionaryAssetAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建加载字典失败事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的加载字典失败事件。</returns>
        public static LoadDictionaryFailureEventArgs Create(ReadDataFailureEventArgs e)
        {
            LoadDictionaryFailureEventArgs loadDictionaryFailureEventArgs = ReferencePool.Acquire<LoadDictionaryFailureEventArgs>();
            loadDictionaryFailureEventArgs.DictionaryAssetAddress = e.DataAssetAddress;
            loadDictionaryFailureEventArgs.ErrorMessage = e.ErrorMessage;
            loadDictionaryFailureEventArgs.UserData = e.UserData;
            return loadDictionaryFailureEventArgs;
        }

        /// <summary>
        /// 清理加载字典失败事件。
        /// </summary>
        public override void Clear()
        {
            DictionaryAssetAddress = AssetAddress.Empty;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
