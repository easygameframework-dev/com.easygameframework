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
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        [Serializable]
        private sealed class EntityGroup
        {
            [SerializeField]
            private string _name = null;

            [SerializeField]
            private float _instanceAutoReleaseInterval = 60f;

            [SerializeField]
            private int _instanceCapacity = 16;

            [SerializeField]
            private float _instanceExpireTime = 60f;

            [SerializeField]
            private int _instancePriority = 0;

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public float InstanceAutoReleaseInterval
            {
                get
                {
                    return _instanceAutoReleaseInterval;
                }
            }

            public int InstanceCapacity
            {
                get
                {
                    return _instanceCapacity;
                }
            }

            public float InstanceExpireTime
            {
                get
                {
                    return _instanceExpireTime;
                }
            }

            public int InstancePriority
            {
                get
                {
                    return _instancePriority;
                }
            }
        }
    }
}
