//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private sealed class HeartBeatState
        {
            private float _heartBeatElapseSeconds;
            private int _missHeartBeatCount;

            public HeartBeatState()
            {
                _heartBeatElapseSeconds = 0f;
                _missHeartBeatCount = 0;
            }

            public float HeartBeatElapseSeconds
            {
                get
                {
                    return _heartBeatElapseSeconds;
                }
                set
                {
                    _heartBeatElapseSeconds = value;
                }
            }

            public int MissHeartBeatCount
            {
                get
                {
                    return _missHeartBeatCount;
                }
                set
                {
                    _missHeartBeatCount = value;
                }
            }

            public void Reset(bool resetHeartBeatElapseSeconds)
            {
                if (resetHeartBeatElapseSeconds)
                {
                    _heartBeatElapseSeconds = 0f;
                }

                _missHeartBeatCount = 0;
            }
        }
    }
}
