using System;
using System.Collections.Generic;
using Drawmasters.Advertising;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.Levels.Helpers;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.Advertising;
using Modules.General.Abstraction;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IAbTestService = Drawmasters.ServiceUtil.Interfaces.IAbTestService;


namespace Drawmasters.Ui
{
    public class ResultLoseBehaviour : IResultBehaviour
    {
        #region Helpers
        
        [Serializable]
        public class Data
        {
            [Required] public GameObject rootObject = default;
            [Required] public GameObject headerObject = default;
            [Required] public TMP_Text indexHeaderText = default;
            [Required] public RewardedVideoButton videoSkipButton = default;
            [Required] public Button freeSkipButton = default;
            [Required] public Button reloadButton = default;
        }
        
        #endregion
        
        
        
        
        #region Fields
        
        private readonly List<GameObject> objects;
        private readonly ResultScreen resultScreen;
        private readonly Data data;

        private readonly ILevelEnvironment levelEnvironment;
        private readonly IAbTestService abTestService;
        private readonly ICommonStatisticsService commonStatistic;

        #endregion



        #region Ctor

        public ResultLoseBehaviour(Data _data, ResultScreen screen)
        {
            data = _data;
            resultScreen = screen;
            objects = new List<GameObject>() { data.rootObject, data.headerObject };

            levelEnvironment = GameServices.Instance.LevelEnvironment;
            abTestService = GameServices.Instance.AbTestService;
            commonStatistic = GameServices.Instance.CommonStatisticService;
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize(){ }

        #endregion



        #region IResultBehaviour

        public void Enable()
        {
            objects.ForEach(go => CommonUtility.SetObjectActive(go, true));

            LevelContext levelContext = levelEnvironment.Context;
            GameMode mode = levelContext.Mode;

            int index = commonStatistic.GetLevelsFinishedCount(mode);

            string indexText = (index + 1).ToString();            

            data.indexHeaderText.text = indexText;

            bool isSkipAvailable = GameServices.Instance.ProposalService.LevelSkipProposal.CanPropose;
            if (isSkipAvailable)
            {
                bool isSkipWithAds = abTestService.CommonData.isSkipLevelNeedRewardVideo;
                if (isSkipWithAds)
                {
                    CommonUtility.SetObjectActive(data.videoSkipButton.gameObject, true);
                    CommonUtility.SetObjectActive(data.freeSkipButton.gameObject, false);
                }
                else
                {
                    CommonUtility.SetObjectActive(data.videoSkipButton.gameObject, false);
                    CommonUtility.SetObjectActive(data.freeSkipButton.gameObject, true);
                }
            }
            else
            {
                CommonUtility.SetObjectActive(data.videoSkipButton.gameObject, false);
                CommonUtility.SetObjectActive(data.freeSkipButton.gameObject, false);
            }
            
            data.videoSkipButton.Initialize(AdsVideoPlaceKeys.SkipLevel);
            data.videoSkipButton.OnVideoShowEnded += VideoSkipButton_OnVideoShowEnded;
            data.videoSkipButton.InitializeButtons();
            
            data.freeSkipButton.onClick.AddListener(FreeSkipButton_OnClicked);
            
            data.reloadButton.onClick.AddListener(ReloadButton_OnClicked);
        }


        public void Disable()
        {
            objects.ForEach(go => CommonUtility.SetObjectActive(go, false));

            DeinitializeButtons();
        }


        public void InitializeButtons() { }

        public void DeinitializeButtons()
        {
            data.videoSkipButton.OnVideoShowEnded -= VideoSkipButton_OnVideoShowEnded;
            data.videoSkipButton.Deinitialize();

            data.freeSkipButton.onClick.RemoveListener(FreeSkipButton_OnClicked);

            data.reloadButton.onClick.RemoveListener(ReloadButton_OnClicked);
        }

        #endregion



        #region Private methods

        private void SkipLevel()
        {
            bool canSkip = LevelsManager.Instance.Level.CurrentState.CanSkipOnResult();
            if (canSkip)
            {
                LevelProgressObserver.TriggerLevelSkip();

                resultScreen.Hide();
            }
        }
        
        #endregion
        
        
        
        #region Events handlers
        
        private void VideoSkipButton_OnVideoShowEnded(AdActionResultType adResult)
        {
            if (adResult == AdActionResultType.Success)
            {
                SkipLevel();
            }
        }


        private void FreeSkipButton_OnClicked()
        {
            SkipLevel();
        }


        private void ReloadButton_OnClicked()
        {
            data.reloadButton.onClick.RemoveListener(ReloadButton_OnClicked);
            data.videoSkipButton.DeinitializeButtons();

            AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial,
                AdPlacementType.InGameRestart, result =>
                {
                    LevelProgressObserver.TriggerLevelReload();

                    resultScreen.Hide();
                });
        }
        
        #endregion
    }
}