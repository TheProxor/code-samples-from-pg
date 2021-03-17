using System;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSeasonEventSmallPropose : UiSeasonEventBasePropose
    {
        #region Fields

        [SerializeField] private Image rewardImage = default;
        
        #endregion



        #region Properties
        
        public override bool ShouldShowProposalRoot =>
            base.ShouldShowProposalRoot &&
            controller.IsActive;

        #endregion



        #region Methods

        protected override void OnShouldRefreshVisual()
        {
            base.OnShouldRefreshVisual();
            
            rewardImage.sprite = controller.IsPetMainReward ?
                IngameData.Settings.pets.uiSettings.FindMainMenuSprite(controller.PetMainRewardType) : 
                controller.VisualSettings.commonRewardSprite;

            // hotfix as there are no complied gem sprite
            if (controller.IsPetMainReward)
            {
                rewardImage.SetNativeSize();
            }
        }

        #endregion
    }
}
