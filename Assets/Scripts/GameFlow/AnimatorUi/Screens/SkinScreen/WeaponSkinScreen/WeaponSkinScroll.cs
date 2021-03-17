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
    public class WeaponSkinScroll : UiSkinScroll<WeaponSkinCard, WeaponSkinType>
    {
        #region Fields
        
        private WeaponType weaponType;
        protected IAlertable alertable;

        #endregion
        
        
        
        #region Properties

        protected virtual IAlertable Alertable =>
            alertable ?? (alertable = GameServices.Instance.ProposalService.VideoWeaponSkinProposal as IAlertable);

        #endregion



        #region Properties
        
        public override IProposable Proposal =>
            GameServices.Instance.ProposalService.VideoWeaponSkinProposal;
        
        public override string VideoPlacementKey =>
            AdsVideoPlaceKeys.WeaponSkinsPanel;
        
        public override UiPanelRewardController Controller =>
            GameServices.Instance.ProposalService.UiPanelWeaponSkinRewardController;

        public override string AnimatorTrigerKey => "Weapon";
        
        #endregion


        
        #region Ctor

        public WeaponSkinScroll(UiSkinScrollData _data)
        {
            data = _data;
        }

        #endregion

        
        
        #region Methods

        public override void Enable()
        {
            weaponType = GameServices.Instance.LevelEnvironment.Context.WeaponType;
            Controller.Settings.SetAvailableWeaponType(weaponType);
            
            Alertable?.OnProposalWasShown();
            
            base.Enable();
            
            GameServices.Instance.PlayerStatisticService.PlayerData.OnWeaponSkinSetted += Refresh;
        }


        public override void Disable()
        {
            GameServices.Instance.PlayerStatisticService.PlayerData.OnWeaponSkinSetted -= Refresh;
            
            base.Disable();
        }
        
        protected override List<WeaponSkinCard> CreateCards()
        {
            List<WeaponSkinCard> result = new List<WeaponSkinCard>();

            List<WeaponSkinType> skinTypes = new List<WeaponSkinType>(GameServices.Instance.ShopService.WeaponSkins.BoughtItems);
            WeaponSkinType[] subscriptionSkinTypes = IngameData.Settings.subscriptionRewardSettings.WeaponSkinTypes;

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

            foreach (WeaponSkinType type in skinTypes)
            {
                WeaponType boughtSkinWeaponType = IngameData.Settings.weaponSkinSettings.GetWeaponType(type);

                if (boughtSkinWeaponType == weaponType)
                {
                    GameObject go = Object.Instantiate(data.cardPrefab, data.cardsRoot);
                    WeaponSkinCard card = go.GetComponent<WeaponSkinCard>();
                    card.SetupType(type);

                    bool isSkinForSubscription = IngameData.Settings.subscriptionRewardSettings.IsSkinForSubscription(type);

                    if (isSkinForSubscription)
                    {
                        card.MarkForSubscription();
                    }

                    result.Add(card);
                }
            }

            foreach (var card in result)
            {
                card.SetupWeaponType(weaponType);
            }

            return result;
        }

        #endregion
    }
}
