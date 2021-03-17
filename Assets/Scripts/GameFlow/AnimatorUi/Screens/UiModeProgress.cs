using System;
using System.Collections.Generic;
using DG.Tweening;
using Drawmasters.Levels.Data;
using Drawmasters.Levels.Order;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class UiModeProgress : MonoBehaviour
    {
        #region Nested types

        private enum State
        {
            None = 0,
            Completed = 1,
            Current = 2,
            NotOpened = 3
        }

        [Serializable]
        private class StateInfo
        {
            public State state = default;
            public Color color = default;
        }

        #endregion



        #region Fields

        [Header("back icons")]
        [SerializeField] private Image currentBackgroundIcon = default;
        [SerializeField] private Image nextBackgroundIcon = default;

        [Header("stages")]
        [SerializeField] private StateInfo[] stateInfo = default;
        [SerializeField] private ColorAnimation currentColorAnimation = default;
        [SerializeField] private VectorAnimation currentScaleAnimation = default;
        [SerializeField] private Image[] stages = default;

        [Header("Ab test parameters")]
        [SerializeField] private Image bossStageImage = default;
        [SerializeField] private Image bonusStageImage = default;

        private List<Image> stageImageList;

        #endregion



        #region Methods

        public void Refresh(GameMode gameMode)
        {
            DOTween.Kill(this);

            stageImageList = new List<Image>(stages);

            bool isBossLevelsEnabled = GameServices.Instance.AbTestService.CommonData.isBossLevelsEnabled;

            bossStageImage.enabled = isBossLevelsEnabled;
            bonusStageImage.enabled = !isBossLevelsEnabled;

            if (isBossLevelsEnabled)
            {
                stageImageList.Add(bossStageImage);
                
            }
            else
            {
                stageImageList.Add(bonusStageImage);
            }
            
            bool isOpen = gameMode.IsModeOpen();
            int currentStageNumber = GameServices.Instance.CommonStatisticService.GetLevelsFinishedCount(gameMode) % stageImageList.Count;

            for (int i = 0; i < stageImageList.Count; i++)
            {
                State stageState;

                if (isOpen)
                {
                    if (i < currentStageNumber)
                    {
                        stageState = State.Completed;
                    }
                    else if (i == currentStageNumber)
                    {
                        stageState = State.Current;
                    }
                    else
                    {
                        stageState = State.NotOpened;
                    }
                }
                else
                {
                    stageState = State.NotOpened;
                }

                StateInfo foundInfo = FindInfo(stageState);

                Image graphic = stageImageList[i];
                graphic.color = foundInfo == null ? Color.black : foundInfo.color;

                if (foundInfo.state == State.Current)
                {
                    currentColorAnimation.Play(color => graphic.color = color, this);
                    currentScaleAnimation.Play(scale => graphic.rectTransform.localScale = scale, this);
                }
                else
                {
                    graphic.rectTransform.localScale = Vector3.one;
                }
            }

            LevelContext context = GameServices.Instance.LevelEnvironment.Context;
            ColorProfile loadedColorProfile = IngameData.Settings.colorProfilesSettings.GetProfile(context.ColorProfileIndex);

            Sprite currentBackgroundSprite = IngameData.Settings.commonBackgroundsSettings.FindUiMainMenuProgressSprite(loadedColorProfile.backgroundIndex);
            currentBackgroundIcon.sprite = currentBackgroundSprite;
            currentBackgroundIcon.SetNativeSize();

            ILevelGraphicService levelGraphicService = GameServices.Instance.LevelGraphicService;

            Sprite nextBackgroundSprite = default;

            if (levelGraphicService.IsLevelProfileExists(context.Mode, context.ChapterIndex + 1, context.Index))
            {
                int nextColorProfileIndex = levelGraphicService.GetLevelProfileIndex(context.Mode, context.ChapterIndex + 1, context.Index);

                if (nextColorProfileIndex == -1)
                {
                    nextBackgroundSprite = IngameData.Settings.commonBackgroundsSettings.RandomUiMainMenuProgressSprites(currentBackgroundSprite);
                }
                else
                {
                    ColorProfile nextColorProfile = IngameData.Settings.colorProfilesSettings.GetProfile(nextColorProfileIndex);
                    nextBackgroundSprite = IngameData.Settings.commonBackgroundsSettings.FindUiMainMenuProgressSprite(nextColorProfile.backgroundIndex);
                }
            }
            else
            {
                nextBackgroundSprite = IngameData.Settings.commonBackgroundsSettings.RandomUiMainMenuProgressSprites(currentBackgroundSprite);
            }

            nextBackgroundIcon.sprite = nextBackgroundSprite;
            nextBackgroundIcon.SetNativeSize();
        }


        public void Deinitialize() =>
            DOTween.Kill(this);


        private StateInfo FindInfo(State state) =>
            Array.Find(stateInfo, e => e.state == state);

        #endregion
    }
}
