
namespace EasyGameFramework
{
    public class SubscriptionOnDestroyTrigger : SubscriptionTriggerBase
    {
        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
