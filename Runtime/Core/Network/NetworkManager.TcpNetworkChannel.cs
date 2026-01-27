//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;

namespace EasyGameFramework.Core.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        /// <summary>
        /// TCP 网络频道。
        /// </summary>
        private sealed class TcpNetworkChannel : NetworkChannelBase
        {
            private readonly AsyncCallback _connectCallback;
            private readonly AsyncCallback _sendCallback;
            private readonly AsyncCallback _receiveCallback;

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public TcpNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
                _connectCallback = ConnectCallback;
                _sendCallback = SendCallback;
                _receiveCallback = ReceiveCallback;
            }

            /// <summary>
            /// 获取网络服务类型。
            /// </summary>
            public override ServiceType ServiceType
            {
                get
                {
                    return ServiceType.Tcp;
                }
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public override void Connect(IPAddress ipAddress, int port, object userData)
            {
                base.Connect(ipAddress, port, userData);
                _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (_socket == null)
                {
                    string errorMessage = "Initialize network channel failure.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                _networkChannelHelper.PrepareForConnecting();
                ConnectAsync(ipAddress, port, userData);
            }

            protected override bool ProcessSend()
            {
                if (base.ProcessSend())
                {
                    SendAsync();
                    return true;
                }

                return false;
            }

            private void ConnectAsync(IPAddress ipAddress, int port, object userData)
            {
                try
                {
                    _socket.BeginConnect(ipAddress, port, _connectCallback, new ConnectState(_socket, userData));
                }
                catch (Exception exception)
                {
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void ConnectCallback(IAsyncResult ar)
            {
                ConnectState socketUserData = (ConnectState)ar.AsyncState;
                try
                {
                    socketUserData.Socket.EndConnect(ar);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    _active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                _sentPacketCount = 0;
                _receivedPacketCount = 0;

                lock (_sendPacketPool)
                {
                    _sendPacketPool.Clear();
                }

                _receivePacketPool.Clear();

                lock (_heartBeatState)
                {
                    _heartBeatState.Reset(true);
                }

                if (NetworkChannelConnected != null)
                {
                    NetworkChannelConnected(this, socketUserData.UserData);
                }

                _active = true;
                ReceiveAsync();
            }

            private void SendAsync()
            {
                try
                {
                    _socket.BeginSend(_sendState.Stream.GetBuffer(), (int)_sendState.Stream.Position, (int)(_sendState.Stream.Length - _sendState.Stream.Position), SocketFlags.None, _sendCallback, _socket);
                }
                catch (Exception exception)
                {
                    _active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void SendCallback(IAsyncResult ar)
            {
                Socket socket = (Socket)ar.AsyncState;
                if (!socket.Connected)
                {
                    return;
                }

                int bytesSent = 0;
                try
                {
                    bytesSent = socket.EndSend(ar);
                }
                catch (Exception exception)
                {
                    _active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                _sendState.Stream.Position += bytesSent;
                if (_sendState.Stream.Position < _sendState.Stream.Length)
                {
                    SendAsync();
                    return;
                }

                _sentPacketCount++;
                _sendState.Reset();
            }

            private void ReceiveAsync()
            {
                try
                {
                    _socket.BeginReceive(_receiveState.Stream.GetBuffer(), (int)_receiveState.Stream.Position, (int)(_receiveState.Stream.Length - _receiveState.Stream.Position), SocketFlags.None, _receiveCallback, _socket);
                }
                catch (Exception exception)
                {
                    _active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                Socket socket = (Socket)ar.AsyncState;
                if (!socket.Connected)
                {
                    return;
                }

                int bytesReceived = 0;
                try
                {
                    bytesReceived = socket.EndReceive(ar);
                }
                catch (Exception exception)
                {
                    _active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                if (bytesReceived <= 0)
                {
                    Close();
                    return;
                }

                _receiveState.Stream.Position += bytesReceived;
                if (_receiveState.Stream.Position < _receiveState.Stream.Length)
                {
                    ReceiveAsync();
                    return;
                }

                _receiveState.Stream.Position = 0L;

                bool processSuccess = false;
                if (_receiveState.PacketHeader != null)
                {
                    processSuccess = ProcessPacket();
                    _receivedPacketCount++;
                }
                else
                {
                    processSuccess = ProcessPacketHeader();
                }

                if (processSuccess)
                {
                    ReceiveAsync();
                    return;
                }
            }
        }
    }
}
