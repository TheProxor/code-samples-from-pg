using Drawmasters.ServiceUtil;
using Drawmasters.Proposal;
using Modules.Sound;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class UiSeasonEventPreviewScreen : UiLiveOpsPreviewScreen
    {
        #region Fields

        [SerializeField] private Localize headerText = default;

        [SerializeField] private GameObject[] gemsGameObjects = default;
        [SerializeField] private Image gemsImage = default;

        [SerializeField] private GameObject[] petGameObjects = default;
        [SerializeField] private SkeletonGraphic petSkeletonGraphic = default;


        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.SeasonEventPreview;

        #endregion



        #region Methods

        public override void Show()
        {
            base.Show();

            SeasonEventProposeController controller = GameServices.Instance.ProposalService.SeasonEventProposeController;
            RefreshHeaderText(controller);

            CommonUtility.SetObjectsActive(petGameObjects, controller.IsPetMainReward);
            petSkeletonGraphic.skeletonDataAsset = controller.VisualSettings.GetPreviewSkeletonGraphic(controller.PetMainRewardType);
            petSkeletonGraphic.Initialize(true);

            CommonUtility.SetObjectsActive(gemsGameObjects, !controller.IsPetMainReward);
            gemsImage.sprite = controller.VisualSettings.previewGemsSprite;
            gemsImage.SetNativeSize();

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.LIVEOPS_POPUP_APPEAR);
        }


        protected override LiveOpsProposeController GetController() =>
            GameServices.Instance.ProposalService.SeasonEventProposeController;


        protected override void OnShouldShowLiveOpsScreen() =>
            UiScreenManager.Instance.ShowScreen(ScreenType.SeasonEventScreen, isForceHideIfExist: true);


        private void RefreshHeaderText(SeasonEventProposeController controller)
        {
            string key = controller.IsPetMainReward ? controller.VisualSettings.FindHeaderKey(controller.PetMainRewardType) : controller.VisualSettings.commonRewardHeaderKey;

            headerText.SetTerm(key);
        }

        #endregion
    }
}
