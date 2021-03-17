namespace Drawmasters.ServiceUtil
{
    public interface INotificationService
    {
        void RegisterAllNotifications();
        void UnregisterAllNotifications();

        void MarkQueryShowed();
    }
}