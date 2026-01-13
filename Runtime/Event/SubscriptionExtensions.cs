using System.Collections.Generic;
using EasyGameFramework.Core.Event;
using UnityEngine;

namespace EasyGameFramework
{
    public static class SubscriptionExtensions
    {
        public static ISubscription UnsubscribeWhenDestroyed(
            this ISubscription subscription,
            GameObject gameObject)
        {
            var trigger = gameObject.GetOrAddComponent<SubscriptionOnDestroyTrigger>();
            trigger.AddUnsubscribe(subscription);
            return subscription;
        }

        public static ISubscription UnsubscribeWhenDisabled(
            this ISubscription subscription,
            GameObject gameObject)
        {
            var trigger = gameObject.GetOrAddComponent<SubscriptionOnDisableTrigger>();
            trigger.AddUnsubscribe(subscription);
            return subscription;
        }
    }
}
