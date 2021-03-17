using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class UiSeasonEventScreenInfo : AnimatorScreen
    {
        #region Fields

        [SerializeField] private Button closeButton = default;
        [SerializeField] private Image commonRewardImage = default;

        #endregion



        #region Overrided properties

        public override ScreenType ScreenType =>
            ScreenType.SeasonEventInfo;

        #endregion



        #region Overrided methods

        public override void Show()
        {
            base.Show();

            RefreshVisual();
        }
        
        
        public override void DeinitializeButtons()
        {
            closeButton.onClick.RemoveListener(Hide);
        }


        public override void InitializeButtons()
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        
        public void RefreshVisual()
        {
            SeasonEventProposeController controller = GameServices.Instance.ProposalService.SeasonEventProposeController;
            
            bool isPetMainReward = controller.IsPetMainReward;
            PetSkinType type = isPetMainReward ? controller.PetMainRewardType : PetSkinType.None;
            
            commonRewardImage.sprite = controller.VisualSettings.GetcommonRewardSprite(type);
            
            commonRewardImage.SetNativeSize();
        }

        #endregion
    }
}
