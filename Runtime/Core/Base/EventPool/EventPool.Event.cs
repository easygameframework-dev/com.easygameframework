//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core
{
    internal sealed partial class EventPool<T> where T : BaseEventArgs
    {
        /// <summary>
        /// 事件结点。
        /// </summary>
        private sealed class Event : IReference
        {
            private object _sender;
            private T _eventArgs;

            public Event()
            {
                _sender = null;
                _eventArgs = null;
            }

            public object Sender
            {
                get
                {
                    return _sender;
                }
            }

            public T EventArgs
            {
                get
                {
                    return _eventArgs;
                }
            }

            public static Event Create(object sender, T e)
            {
                Event eventNode = ReferencePool.Acquire<Event>();
                eventNode._sender = sender;
                eventNode._eventArgs = e;
                return eventNode;
            }

            public void Clear()
            {
                _sender = null;
                _eventArgs = null;
            }
        }
    }
}
