//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Net.Sockets;

namespace EasyGameFramework.Core.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private sealed class ConnectState
        {
            private readonly Socket _socket;
            private readonly object _userData;

            public ConnectState(Socket socket, object userData)
            {
                _socket = socket;
                _userData = userData;
            }

            public Socket Socket
            {
                get
                {
                    return _socket;
                }
            }

            public object UserData
            {
                get
                {
                    return _userData;
                }
            }
        }
    }
}
