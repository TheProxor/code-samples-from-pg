using System;
using Drawmasters.Proposal;


namespace Drawmasters.Notifications
{
    public class LiveOpsStartNextNotification : Notification
    {
        private readonly ILiveOpsNextStartNotification liveOpsFinishSoonNotification;

        public override bool AllowToRegister
        {
            get
            {
                return liveOpsFinishSoonNotification.AllowRegisterNextStartNotification &&
                       liveOpsFinishSoonNotification.FireDateTimeNextStartNotification < DateTime.MaxValue;
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

                return liveOpsFinishSoonNotification.FireDateTimeNextStartNotification;
            }
        }



        public LiveOpsStartNextNotification(ILiveOpsNextStartNotification _liveOpsFinishSoonNotification, string _id, string _message, int _fireCount = 1, string _title = null) :
            base(_id, _message, _fireCount, _title)
        {
            liveOpsFinishSoonNotification = _liveOpsFinishSoonNotification;
        }
    }
}