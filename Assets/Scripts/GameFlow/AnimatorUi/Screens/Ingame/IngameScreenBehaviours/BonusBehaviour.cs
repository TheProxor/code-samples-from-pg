using System;
using Drawmasters.Announcer;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class BonusBehaviour : BaseIngameScreenBehaviour
    {
        #region Helpers
        
        [Serializable]
        public class BonusData
        {
            [Required] public Transform bonusLevelAnimatable = default;
            [Required] public Transform startDrawAnnouncer = default;
            [Required] public Transform finishLevelAnimatable = default;
            [Required] public Transform collectPetAnimatable = default;
        }
        
        #endregion
        
        
        
        #region Fields
        
        private readonly BonusData bonusData;
        private readonly UiHudTop uiHudTop;
        private readonly BonusLevelVfxHelper vfxHelper;
        
        #endregion

        
        
        #region Ctor

        public BonusBehaviour(IngameScreen ingameScreen, 
                              Data _data,
                              BonusData _bonusData,
                              UiHudTop _uiHudTop) 
            : base(ingameScreen, _data)
        {
            bonusData = _bonusData;
            uiHudTop = _uiHudTop;
            
            vfxHelper = new BonusLevelVfxHelper(GameServices.Instance.LevelControllerService.BonusLevelController, 
                ingameScreen);
        }
        
        #endregion
        
        
        
        #region Abstract implementation

        protected override Announcer.Announcer[] AvailableAnnouncers
            => new Announcer.Announcer[] 
            {
                new BonusLevelAnnouncer(screen, bonusData.bonusLevelAnimatable),
                new BonusLevelStartDrawAnnouncer(bonusData.startDrawAnnouncer),
                new BonusLevelFinishedAnnouncer(bonusData.finishLevelAnimatable),
                new BonusLevelCollectPetAnnouncer(bonusData.collectPetAnimatable)
            };

        protected override bool IsSkipButtonAvailable => false;

        public override bool IsCallPetButtonAvailable => false;

        #endregion



        #region Overrided methods

        public override void Initialize()
        {
            base.Initialize();
            
            CommonUtility.SetObjectActive(uiHudTop.gameObject, true);
            CommonUtility.SetObjectActive(data.reloadButton.gameObject, false);
            CommonUtility.SetObjectActive(data.levelNumberText.gameObject, false);
            CommonUtility.SetObjectActive(data.stagesHandler.gameObject, false);
            CommonUtility.SetObjectActive(data.pauseButton.gameObject, false);    
            
            //HACK
            uiHudTop.InitializeCurrencyRefresh();
            uiHudTop.RefreshCurrencyVisual(0f);
            uiHudTop.DeinitializeCurrencyRefresh();

            uiHudTop.Initialize();
            
            vfxHelper.Initialize();
        }


        public override void Deinitialize()
        {
            uiHudTop.DeinitializeCurrencyRefresh();
            vfxHelper.Deinitialize();
            uiHudTop.Deinitialize();
            
            base.Deinitialize();
        }

        #endregion
    }
}

