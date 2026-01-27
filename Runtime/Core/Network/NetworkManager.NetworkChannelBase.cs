//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EasyGameFramework.Core.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        /// <summary>
        /// 网络频道基类。
        /// </summary>
        private abstract class NetworkChannelBase : INetworkChannel, IDisposable
        {
            private const float DefaultHeartBeatInterval = 30f;

            private readonly string _name;
            protected readonly Queue<Packet> _sendPacketPool;
            protected readonly EventPool<Packet> _receivePacketPool;
            protected readonly INetworkChannelHelper _networkChannelHelper;
            protected AddressFamily _addressFamily;
            protected bool _resetHeartBeatElapseSecondsWhenReceivePacket;
            protected float _heartBeatInterval;
            protected Socket _socket;
            protected readonly SendState _sendState;
            protected readonly ReceiveState _receiveState;
            protected readonly HeartBeatState _heartBeatState;
            protected int _sentPacketCount;
            protected int _receivedPacketCount;
            protected bool _active;
            private bool _disposed;

            public GameFrameworkAction<NetworkChannelBase, object> NetworkChannelConnected;
            public GameFrameworkAction<NetworkChannelBase> NetworkChannelClosed;
            public GameFrameworkAction<NetworkChannelBase, int> NetworkChannelMissHeartBeat;
            public GameFrameworkAction<NetworkChannelBase, NetworkErrorCode, SocketError, string> NetworkChannelError;
            public GameFrameworkAction<NetworkChannelBase, object> NetworkChannelCustomError;

            /// <summary>
            /// 初始化网络频道基类的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public NetworkChannelBase(string name, INetworkChannelHelper networkChannelHelper)
            {
                _name = name ?? string.Empty;
                _sendPacketPool = new Queue<Packet>();
                _receivePacketPool = new EventPool<Packet>(EventPoolMode.Default);
                _networkChannelHelper = networkChannelHelper;
                _addressFamily = AddressFamily.Unknown;
                _resetHeartBeatElapseSecondsWhenReceivePacket = false;
                _heartBeatInterval = DefaultHeartBeatInterval;
                _socket = null;
                _sendState = new SendState();
                _receiveState = new ReceiveState();
                _heartBeatState = new HeartBeatState();
                _sentPacketCount = 0;
                _receivedPacketCount = 0;
                _active = false;
                _disposed = false;

                NetworkChannelConnected = null;
                NetworkChannelClosed = null;
                NetworkChannelMissHeartBeat = null;
                NetworkChannelError = null;
                NetworkChannelCustomError = null;

                networkChannelHelper.Initialize(this);
            }

            /// <summary>
            /// 获取网络频道名称。
            /// </summary>
            public string Name
            {
                get
                {
                    return _name;
                }
            }

            /// <summary>
            /// 获取网络频道所使用的 Socket。
            /// </summary>
            public Socket Socket
            {
                get
                {
                    return _socket;
                }
            }

            /// <summary>
            /// 获取是否已连接。
            /// </summary>
            public bool Connected
            {
                get
                {
                    if (_socket != null)
                    {
                        return _socket.Connected;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 获取网络服务类型。
            /// </summary>
            public abstract ServiceType ServiceType
            {
                get;
            }

            /// <summary>
            /// 获取网络地址类型。
            /// </summary>
            public AddressFamily AddressFamily
            {
                get
                {
                    return _addressFamily;
                }
            }

            /// <summary>
            /// 获取要发送的消息包数量。
            /// </summary>
            public int SendPacketCount
            {
                get
                {
                    return _sendPacketPool.Count;
                }
            }

            /// <summary>
            /// 获取累计发送的消息包数量。
            /// </summary>
            public int SentPacketCount
            {
                get
                {
                    return _sentPacketCount;
                }
            }

            /// <summary>
            /// 获取已接收未处理的消息包数量。
            /// </summary>
            public int ReceivePacketCount
            {
                get
                {
                    return _receivePacketPool.EventCount;
                }
            }

            /// <summary>
            /// 获取累计已接收的消息包数量。
            /// </summary>
            public int ReceivedPacketCount
            {
                get
                {
                    return _receivedPacketCount;
                }
            }

            /// <summary>
            /// 获取或设置当收到消息包时是否重置心跳流逝时间。
            /// </summary>
            public bool ResetHeartBeatElapseSecondsWhenReceivePacket
            {
                get
                {
                    return _resetHeartBeatElapseSecondsWhenReceivePacket;
                }
                set
                {
                    _resetHeartBeatElapseSecondsWhenReceivePacket = value;
                }
            }

            /// <summary>
            /// 获取丢失心跳的次数。
            /// </summary>
            public int MissHeartBeatCount
            {
                get
                {
                    return _heartBeatState.MissHeartBeatCount;
                }
            }

            /// <summary>
            /// 获取或设置心跳间隔时长，以秒为单位。
            /// </summary>
            public float HeartBeatInterval
            {
                get
                {
                    return _heartBeatInterval;
                }
                set
                {
                    _heartBeatInterval = value;
                }
            }

            /// <summary>
            /// 获取心跳等待时长，以秒为单位。
            /// </summary>
            public float HeartBeatElapseSeconds
            {
                get
                {
                    return _heartBeatState.HeartBeatElapseSeconds;
                }
            }

            /// <summary>
            /// 网络频道轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public virtual void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (_socket == null || !_active)
                {
                    return;
                }

                ProcessSend();
                ProcessReceive();
                if (_socket == null || !_active)
                {
                    return;
                }

                _receivePacketPool.Update(elapseSeconds, realElapseSeconds);

                if (_heartBeatInterval > 0f)
                {
                    bool sendHeartBeat = false;
                    int missHeartBeatCount = 0;
                    lock (_heartBeatState)
                    {
                        if (_socket == null || !_active)
                        {
                            return;
                        }

                        _heartBeatState.HeartBeatElapseSeconds += realElapseSeconds;
                        if (_heartBeatState.HeartBeatElapseSeconds >= _heartBeatInterval)
                        {
                            sendHeartBeat = true;
                            missHeartBeatCount = _heartBeatState.MissHeartBeatCount;
                            _heartBeatState.HeartBeatElapseSeconds = 0f;
                            _heartBeatState.MissHeartBeatCount++;
                        }
                    }

                    if (sendHeartBeat && _networkChannelHelper.SendHeartBeat())
                    {
                        if (missHeartBeatCount > 0 && NetworkChannelMissHeartBeat != null)
                        {
                            NetworkChannelMissHeartBeat(this, missHeartBeatCount);
                        }
                    }
                }
            }

            /// <summary>
            /// 关闭网络频道。
            /// </summary>
            public virtual void Shutdown()
            {
                Close();
                _receivePacketPool.Shutdown();
                _networkChannelHelper.Shutdown();
            }

            /// <summary>
            /// 注册网络消息包处理函数。
            /// </summary>
            /// <param name="handler">要注册的网络消息包处理函数。</param>
            public void RegisterHandler(IPacketHandler handler)
            {
                if (handler == null)
                {
                    throw new GameFrameworkException("Packet handler is invalid.");
                }

                _receivePacketPool.Subscribe(handler.Id, handler.Handle);
            }

            /// <summary>
            /// 设置默认事件处理函数。
            /// </summary>
            /// <param name="handler">要设置的默认事件处理函数。</param>
            public void SetDefaultHandler(EventHandler<Packet> handler)
            {
                _receivePacketPool.SetDefaultHandler(handler);
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            public void Connect(IPAddress ipAddress, int port)
            {
                Connect(ipAddress, port, null);
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public virtual void Connect(IPAddress ipAddress, int port, object userData)
            {
                if (_socket != null)
                {
                    Close();
                    _socket = null;
                }

                switch (ipAddress.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        _addressFamily = AddressFamily.IPv4;
                        break;

                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        _addressFamily = AddressFamily.IPv6;
                        break;

                    default:
                        string errorMessage = Utility.Text.Format("Not supported address family '{0}'.", ipAddress.AddressFamily);
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.AddressFamilyError, SocketError.Success, errorMessage);
                            return;
                        }

                        throw new GameFrameworkException(errorMessage);
                }

                _sendState.Reset();
                _receiveState.PrepareForPacketHeader(_networkChannelHelper.PacketHeaderLength);
            }

            /// <summary>
            /// 关闭连接并释放所有相关资源。
            /// </summary>
            public void Close()
            {
                lock (this)
                {
                    if (_socket == null)
                    {
                        return;
                    }

                    _active = false;

                    try
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _socket.Close();
                        _socket = null;

                        if (NetworkChannelClosed != null)
                        {
                            NetworkChannelClosed(this);
                        }
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
                }
            }

            /// <summary>
            /// 向远程主机发送消息包。
            /// </summary>
            /// <typeparam name="T">消息包类型。</typeparam>
            /// <param name="packet">要发送的消息包。</param>
            public void Send<T>(T packet) where T : Packet
            {
                if (_socket == null)
                {
                    string errorMessage = "You must connect first.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (!_active)
                {
                    string errorMessage = "Socket is not active.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (packet == null)
                {
                    string errorMessage = "Packet is invalid.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                lock (_sendPacketPool)
                {
                    _sendPacketPool.Enqueue(packet);
                }
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            /// <param name="disposing">释放资源标记。</param>
            private void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    Close();
                    _sendState.Dispose();
                    _receiveState.Dispose();
                }

                _disposed = true;
            }

            protected virtual bool ProcessSend()
            {
                if (_sendState.Stream.Length > 0 || _sendPacketPool.Count <= 0)
                {
                    return false;
                }

                while (_sendPacketPool.Count > 0)
                {
                    Packet packet = null;
                    lock (_sendPacketPool)
                    {
                        packet = _sendPacketPool.Dequeue();
                    }

                    bool serializeResult = false;
                    try
                    {
                        serializeResult = _networkChannelHelper.Serialize(packet, _sendState.Stream);
                    }
                    catch (Exception exception)
                    {
                        _active = false;
                        if (NetworkChannelError != null)
                        {
                            SocketException socketException = exception as SocketException;
                            NetworkChannelError(this, NetworkErrorCode.SerializeError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                            return false;
                        }

                        throw;
                    }

                    if (!serializeResult)
                    {
                        string errorMessage = "Serialized packet failure.";
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.SerializeError, SocketError.Success, errorMessage);
                            return false;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }
                }

                _sendState.Stream.Position = 0L;
                return true;
            }

            protected virtual void ProcessReceive()
            {
            }

            protected virtual bool ProcessPacketHeader()
            {
                try
                {
                    object customErrorData = null;
                    IPacketHeader packetHeader = _networkChannelHelper.DeserializePacketHeader(_receiveState.Stream, out customErrorData);

                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    if (packetHeader == null)
                    {
                        string errorMessage = "Packet header is invalid.";
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, SocketError.Success, errorMessage);
                            return false;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }

                    _receiveState.PrepareForPacket(packetHeader);
                    if (packetHeader.PacketLength <= 0)
                    {
                        bool processSuccess = ProcessPacket();
                        _receivedPacketCount++;
                        return processSuccess;
                    }
                }
                catch (Exception exception)
                {
                    _active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }

            protected virtual bool ProcessPacket()
            {
                lock (_heartBeatState)
                {
                    _heartBeatState.Reset(_resetHeartBeatElapseSecondsWhenReceivePacket);
                }

                try
                {
                    object customErrorData = null;
                    Packet packet = _networkChannelHelper.DeserializePacket(_receiveState.PacketHeader, _receiveState.Stream, out customErrorData);

                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    if (packet != null)
                    {
                        _receivePacketPool.Fire(this, packet);
                    }

                    _receiveState.PrepareForPacketHeader(_networkChannelHelper.PacketHeaderLength);
                }
                catch (Exception exception)
                {
                    _active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }
        }
    }
}
