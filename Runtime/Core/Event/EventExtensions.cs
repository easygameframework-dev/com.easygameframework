using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EasyGameFramework.Core.Event
{
    public static class EventExtensions
    {
        private static int s_nextEventId = 1000;
        private static readonly ConcurrentDictionary<Type, int> s_eventIdByType = new ConcurrentDictionary<Type, int>();
        private static readonly Dictionary<Delegate, EventHandler<GameEventArgs>> s_handlers =
            new Dictionary<Delegate, EventHandler<GameEventArgs>>();

        private struct EventIdFastGetter<T>
        {
            public static readonly int s_eventId = GetEventId(typeof(T));
        }

        public static int GetEventId(Type eventType)
        {
            return s_eventIdByType.GetOrAdd(eventType, _ => Interlocked.Increment(ref s_nextEventId));
        }

        public static ISubscription Subscribe(this EventComponent eventComponent, Type eventType,
            EventHandler<GameEventArgs> handler)
        {
            if (!s_handlers.TryAdd(handler, handler))
            {
                throw new ArgumentException($"Handler '{handler}' already exists.");
            }

            int eventId = GetEventId(eventType);
            eventComponent.Subscribe(eventId, handler);
            return new Subscription(() =>
            {
                eventComponent.Unsubscribe(eventId, handler);
                s_handlers.Remove(handler);
            });
        }

        public static ISubscription Subscribe<T>(this EventComponent eventComponent, EventHandler<T> handler)
            where T : GameEventArgs
        {
            if (!s_handlers.TryAdd(handler, Handler))
            {
                throw new ArgumentException($"Handler '{handler}' already exists.");
            }

            int eventId = EventIdFastGetter<T>.s_eventId;
            eventComponent.Subscribe(eventId, Handler);
            return new Subscription(() =>
            {
                eventComponent.Unsubscribe(eventId, Handler);
                s_handlers.Remove(handler);
            });

            void Handler(object sender, GameEventArgs e)
            {
                handler(sender, (T)e);
            }
        }

        public static void Unsubscribe(this EventComponent eventComponent, Type eventType,
            EventHandler<GameEventArgs> handler)
        {
            if (s_handlers.TryGetValue(handler, out var eventHandler))
            {
                int eventId = GetEventId(eventType);
                eventComponent.Unsubscribe(eventId, eventHandler);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsubscribe<{eventType}> must corresponds to Subscribe<{eventType}>");
            }
        }

        public static void Unsubscribe<T>(this EventComponent eventComponent, EventHandler<T> handler)
            where T : GameEventArgs
        {
            if (s_handlers.TryGetValue(handler, out var eventHandler))
            {
                int eventId = EventIdFastGetter<T>.s_eventId;
                eventComponent.Unsubscribe(eventId, eventHandler);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsubscribe<{typeof(T)}> must corresponds to Subscribe<{typeof(T)}>");
            }
        }
    }
}
