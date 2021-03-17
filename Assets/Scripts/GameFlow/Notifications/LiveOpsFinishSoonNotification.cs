using System;
using Drawmasters.Proposal;


namespace Drawmasters.Notifications
{
    public class LiveOpsFinishSoonNotification : Notification
    {
        private readonly ILiveOpsFinishSoonNotification liveOpsFinishSoonNotification;

        public override bool AllowToRegister
        {
            get
            {
                return liveOpsFinishSoonNotification.AllowRegisterFinishSoonNotification &&
                       liveOpsFinishSoonNotification.FireDateTimeFinishSoonNotification < DateTime.MaxValue;
            }
        }


        protected override DateTime FireDateTime
        {
            get
            {
                if (!AllowToRegister)
                {
                    CustomDebug.Log($"Trying to get fire date time for not allowed notification");
                }

                return liveOpsFinishSoonNotification.FireDateTimeFinishSoonNotification;
            }
        }



        public LiveOpsFinishSoonNotification(ILiveOpsFinishSoonNotification _liveOpsFinishSoonNotification, string _id, string _message, int _fireCount = 1, string _title = null) :
            base(_id, _message, _fireCount, _title)
        {
            liveOpsFinishSoonNotification = _liveOpsFinishSoonNotification;
        }
    }
}