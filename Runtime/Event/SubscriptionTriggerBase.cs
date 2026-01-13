using System.Collections.Generic;
using EasyGameFramework.Core.Event;
using UnityEngine;

namespace EasyGameFramework
{
    public abstract class SubscriptionTriggerBase : MonoBehaviour
    {
        private readonly HashSet<ISubscription> m_Unsubscribes = new HashSet<ISubscription>();

        public void AddUnsubscribe(ISubscription subscription) => m_Unsubscribes.Add(subscription);

        public void RemoveUnsubscribe(ISubscription subscription) => m_Unsubscribes.Remove(subscription);

        public void Unsubscribe()
        {
            foreach (var unsubscribe in m_Unsubscribes)
            {
                unsubscribe.Unsubscribe();
            }

            m_Unsubscribes.Clear();
        }
    }

}
