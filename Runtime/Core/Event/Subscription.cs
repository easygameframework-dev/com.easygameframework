using System;

namespace EasyGameFramework.Core.Event
{
    public class Subscription : ISubscription
    {
        private readonly Action _onUnsubscribe;

        public Subscription(Action onUnsubscribe)
        {
            _onUnsubscribe = onUnsubscribe;
        }

        public void Unsubscribe()
        {
            _onUnsubscribe?.Invoke();
        }
    }
}
