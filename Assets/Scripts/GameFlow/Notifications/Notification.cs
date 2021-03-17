using System;
using Modules.General.Abstraction;


namespace Drawmasters.Notifications
{
    public abstract class Notification
    {
        private readonly int notificationsCount;
        private readonly string title;
        private readonly string text;



        public string Id { get; }

        public abstract bool AllowToRegister { get; }

        protected abstract DateTime FireDateTime { get; }



        public Notification(string _notificationId, string _notificationText, int _notificationsCount = 1, string _notificationTitle = default)
        {
            Id = _notificationId;
            text = _notificationText;
            title = _notificationTitle;
            notificationsCount = _notificationsCount;
        }


        public NotificationData GenerateData()
        {
            NotificationData result = new NotificationData(Id, text, FireDateTime, notificationsCount, title);
            return result;
        }
    }
}