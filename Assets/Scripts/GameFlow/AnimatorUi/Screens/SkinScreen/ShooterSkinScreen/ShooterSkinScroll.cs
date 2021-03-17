using UnityEngine;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Advertising;
using Drawmasters.ServiceUtil;
using Modules.General.InAppPurchase;
using System.Collections.Generic;
using System.Linq;
using System;
using Object = UnityEngine.Object;


namespace Drawmasters.Ui
{
    public class ShooterSkinScroll : UiSkinScroll<ShooterSkinCard, ShooterSkinType>
    {
        #region Fields
        
        protected IAlertable alertable;

        #endregion
        
        #region Properties

        protected virtual IAlertable Alertable =>
            alertable ?? (alertable = GameServices.Instance.ProposalService.VideoShooterSkinProposal as IAlertable);

        #endregion
        
        
        #region Overrided properties

        public override IProposable Proposal =>
            GameServices.Instance.ProposalService.VideoShooterSkinProposal;
        
        public override string VideoPlacementKey =>
            AdsVideoPlaceKeys.ShooterSkinsPanel;

        public override UiPanelRewardController Controller =>
            GameServices.Instance.ProposalService.UiPanelShooterSkinRewardController;

        public override string AnimatorTrigerKey => "Shooter";
        
        #endregion


        
        #region Ctor

        public ShooterSkinScroll(UiSkinScrollData _data)
        {
            data = _data;
        }

        #endregion
        
        

        #region Methods

        public override void Enable()
        {
            base.Enable();
            
            Alertable?.OnProposalWasShown();
            
            GameServices.Instance.PlayerStatisticService.PlayerData.OnShooterSkinSetted += Refresh;
        }


        public override void Disable()
        {
            GameServices.Instance.PlayerStatisticService.PlayerData.OnShooterSkinSetted -= Refresh;

            base.Disable();
        }


        protected override List<ShooterSkinCard> CreateCards()
        {
            List<ShooterSkinCard> result = new List<ShooterSkinCard>();

            List<ShooterSkinType> skinTypes = new List<ShooterSkinType>(GameServices.Instance.ShopService.ShooterSkins.BoughtItems);
            ShooterSkinType[] subscriptionSkinTypes = IngameData.Settings.subscriptionRewardSettings.ShooterSkinTypes;

            bool shouldProposeSubscription = GameServices.Instance.AbTestService.CommonData.isSubscriptionAvailable &&
                                             GameServices.Instance.CommonStatisticService.IsIapsAvailable;

            if (shouldProposeSubscription)
            {
                if (!SubscriptionManager.Instance.IsSubscriptionActive)
                {
                    skinTypes.AddRange(subscriptionSkinTypes);
                }

                skinTypes = skinTypes.Where(e => !Array.Exists(subscriptionSkinTypes, t => e == t)).ToList();
                skinTypes.InsertRange(0, subscriptionSkinTypes);
            }

            foreach (var type in skinTypes)
            {
                GameObject go = Object.Instantiate(data.cardPrefab, data.cardsRoot);
                ShooterSkinCard card = go.GetComponent<ShooterSkinCard>();

                card.SetupType(type);

                bool isSkinForSubscription = IngameData.Settings.subscriptionRewardSettings.IsSkinForSubscription(type);

                if (isSkinForSubscription)
                {
                    card.MarkForSubscription();
                }

                result.Add(card);
            }


            return result;
        }

        #endregion
    }
}
