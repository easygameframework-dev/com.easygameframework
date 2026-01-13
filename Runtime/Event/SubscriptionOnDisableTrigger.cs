
namespace EasyGameFramework
{
    public class SubscriptionOnDisableTrigger : SubscriptionTriggerBase
    {
        private void OnDisable()
        {
            Unsubscribe();
        }
    }
}
