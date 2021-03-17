using System;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Notifications
{
    public class FreeSpinNotification : Notification
    {
        public override bool AllowToRegister
        {
            get
            {
                SpinRouletteController spinRoulette = GameServices.Instance.ProposalService.SpinRouletteController;
                return !spinRoulette.IsFreeSpinAvailable;
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

                SpinRouletteController spinRoulette = GameServices.Instance.ProposalService.SpinRouletteController;
                return spinRoulette.NextRefreshDate;
            }
        }



        public FreeSpinNotification(string _id, string _message, int _fireCount = 1, string _title = null) :
            base(_id, _message, _fireCount, _title)
        {
        }
    }
}