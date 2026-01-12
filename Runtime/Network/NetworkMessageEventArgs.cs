using EasyGameFramework.Core;
using EasyGameFramework.Core.Event;
using EasyGameFramework.Core.Network;

namespace EasyGameFramework
{
    public class NetworkMessageEventArgs : GameEventArgs
    {
        /// <summary>
        /// 获取网络频道。
        /// </summary>
        public INetworkChannel NetworkChannel { get; private set; }

        public object Message { get; private set; }

        public static NetworkMessageEventArgs Create(INetworkChannel networkChannel, object message)
        {
            var eventArgs = ReferencePool.Acquire<NetworkMessageEventArgs>();
            eventArgs.NetworkChannel = networkChannel;
            eventArgs.Message = message;
            return eventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
            Message = null;
        }
    }
}