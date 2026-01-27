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
    public sealed partial class SoundComponent : GameFrameworkComponent
    {
        [Serializable]
        private sealed class SoundGroup
        {
            [SerializeField]
            private string _name = null;

            [SerializeField]
            private bool _avoidBeingReplacedBySamePriority = false;

            [SerializeField]
            private bool _mute = false;

            [SerializeField, Range(0f, 1f)]
            private float _volume = 1f;

            [SerializeField]
            private int _agentHelperCount = 1;

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public bool AvoidBeingReplacedBySamePriority
            {
                get
                {
                    return _avoidBeingReplacedBySamePriority;
                }
            }

            public bool Mute
            {
                get
                {
                    return _mute;
                }
            }

            public float Volume
            {
                get
                {
                    return _volume;
                }
            }

            public int AgentHelperCount
            {
                get
                {
                    return _agentHelperCount;
                }
            }
        }
    }
}
