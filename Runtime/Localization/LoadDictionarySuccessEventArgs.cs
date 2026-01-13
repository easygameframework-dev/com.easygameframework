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
    /// 加载字典成功事件。
    /// </summary>
    public sealed class LoadDictionarySuccessEventArgs : GameEventArgs
    {
        /// <summary>
        /// 初始化加载字典成功事件的新实例。
        /// </summary>
        public LoadDictionarySuccessEventArgs()
        {
            DictionaryAssetAddress = AssetAddress.Empty;
            Duration = 0f;
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
        /// 获取加载持续时间。
        /// </summary>
        public float Duration
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
        /// 创建加载字典成功事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的加载字典成功事件。</returns>
        public static LoadDictionarySuccessEventArgs Create(ReadDataSuccessEventArgs e)
        {
            LoadDictionarySuccessEventArgs loadDictionarySuccessEventArgs = ReferencePool.Acquire<LoadDictionarySuccessEventArgs>();
            loadDictionarySuccessEventArgs.DictionaryAssetAddress = e.DataAssetAddress;
            loadDictionarySuccessEventArgs.Duration = e.Duration;
            loadDictionarySuccessEventArgs.UserData = e.UserData;
            return loadDictionarySuccessEventArgs;
        }

        /// <summary>
        /// 清理加载字典成功事件。
        /// </summary>
        public override void Clear()
        {
            DictionaryAssetAddress = AssetAddress.Empty;
            Duration = 0f;
            UserData = null;
        }
    }
}
