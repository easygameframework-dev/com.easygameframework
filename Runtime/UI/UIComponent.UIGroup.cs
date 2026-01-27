//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using UnityEngine;

namespace EasyGameFramework
{
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        [Serializable]
        private sealed class UIGroup
        {
            [SerializeField]
            private string _name = null;

            [SerializeField]
            private int _depth = 0;

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public int Depth
            {
                get
                {
                    return _depth;
                }
            }
        }
    }
}
