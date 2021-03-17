using DG.Tweening;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui;
using Drawmasters.Ui.Hitmasters;
using UnityEngine;


namespace Drawmasters.Announcer
{
    public class HitmastersLiveOpsCompleteAnnouncer : Announcer
    {
        #region Fields

        protected override VectorAnimation AnnouncerAnimation => IngameData.Settings.announcerSettings.commonAnnouncer;

        private readonly UiHitmastersMapScreen uiHitmastersMapScreen;

        #endregion



        #region Ctor

        public HitmastersLiveOpsCompleteAnnouncer(UiHitmastersMapScreen _uiHitmastersMapScreen, Transform _liveOpsEndAnimatable) : base(_liveOpsEndAnimatable)
        {
            uiHitmastersMapScreen = _uiHitmastersMapScreen;


            HitmastersProposeController controller = GameServices.Instance.ProposalService.HitmastersProposeController;
            if (controller.IsCurrentLiveOpsCompleted)
            {
                uiHitmastersMapScreen.OnShowBegin += UiHitmastersMapScreen_OnShowBegin;
            }
        }

        #endregion



        #region IDeinitializable

        public override void Deinitialize()
        {
            uiHitmastersMapScreen.OnShowBegin -= UiHitmastersMapScreen_OnShowBegin;

            DOTween.Kill(this);
        }

        #endregion



        #region Events handlers

        private void UiHitmastersMapScreen_OnShowBegin(AnimatorView view) =>
            Ready(this);

        #endregion
    }
}
