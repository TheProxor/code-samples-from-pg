using System;


namespace Drawmasters.Notifications
{
    public class UserMissingNotification : Notification
    {
        private readonly int missedDays;


        public override bool AllowToRegister => true;

        protected override DateTime FireDateTime =>
            DateTime.Now.AddDays(missedDays);


        public UserMissingNotification(int _missedDays, string _id, string _message, int _fireCount = 1, string _title = null) :
            base(_id, _message, _fireCount, _title)
        {
            missedDays = _missedDays;
        }
    }
}
