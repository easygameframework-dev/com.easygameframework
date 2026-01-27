//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;

namespace EasyGameFramework
{
    /// <summary>
    /// 界面字符型主键。
    /// </summary>
    public sealed class UIStringKey : MonoBehaviour
    {
        [SerializeField]
        private string _key = null;

        /// <summary>
        /// 获取或设置主键。
        /// </summary>
        public string Key
        {
            get
            {
                return _key ?? string.Empty;
            }
            set
            {
                _key = value;
            }
        }
    }
}
