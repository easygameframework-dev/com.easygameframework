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
    /// 界面整型主键。
    /// </summary>
    public sealed class UIIntKey : MonoBehaviour
    {
        [SerializeField]
        private int _key = 0;

        /// <summary>
        /// 获取或设置主键。
        /// </summary>
        public int Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }
    }
}
