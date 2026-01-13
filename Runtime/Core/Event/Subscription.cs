using System;

namespace EasyGameFramework.Core.Event
{
    public class Subscription : ISubscription
    {
        private readonly Action m_OnUnsubscribe;

        public Subscription(Action onUnsubscribe)
        {
            m_OnUnsubscribe = onUnsubscribe;
        }

        public void Unsubscribe()
        {
            m_OnUnsubscribe?.Invoke();
        }
    }
}
