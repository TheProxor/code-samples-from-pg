using UnityEngine;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Advertising;
using Drawmasters.ServiceUtil;
using System.Collections.Generic;


namespace Drawmasters.Ui
{
    public class PetSkinScroll : UiSkinScroll<PetSkinCard, PetSkinType>
    {
        #region Properties

        public override IProposable Proposal => null;
        
        public override string VideoPlacementKey =>
            AdsVideoPlaceKeys.PetSkinsPanel;
        
        public override UiPanelRewardController Controller =>
            GameServices.Instance.ProposalService.UiPanelShooterSkinRewardController;

        public override string AnimatorTrigerKey => "Pet";
        
        #endregion


        
        #region Ctor

        public PetSkinScroll(UiSkinScrollData _data)
        {
            data = _data;
        }

        #endregion

        
        
        #region Methods

        public override void Enable()
        {
            base.Enable();
            
            GameServices.Instance.PlayerStatisticService.PlayerData.OnPetSkinSetted += Refresh;
        }


        public override void Disable()
        {
            GameServices.Instance.PlayerStatisticService.PlayerData.OnPetSkinSetted -= Refresh;
            
            base.Disable();
        }

        
        protected override List<PetSkinCard> CreateCards()
        {
            List<PetSkinCard> result = new List<PetSkinCard>();
            List<PetSkinType> skinTypes = new List<PetSkinType>(PetSkinsShop.UserSkinTypes);
            
            foreach (var type in skinTypes)
            {
                GameObject go = Object.Instantiate(data.cardPrefab, data.cardsRoot);
                PetSkinCard card = go.GetComponent<PetSkinCard>();
                card.SetupType(type);

                result.Add(card);
            }
            
            return result;
        }

        #endregion
    }
}
