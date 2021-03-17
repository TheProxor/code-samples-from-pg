using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using Modules.Sound;
using Modules.General;

namespace Drawmasters.Ui
{
    public class UiLeagueIntermediateRewardProgressBar : UiProgressBar
    {
        #region Nested types

        [Serializable]
        private class ChestData
        {
            public Image image = default;
            public Animator animator = default;

#warning Crunch because we can't canculate current stage wherever we want. To Dmitry S
            public float progressTreshholdToBounce = default;
        }

        #endregion



        #region Fields

        [SerializeField] private ChestData[] chestsData = default;
        [SerializeField] private TMP_Text stageText = default;

        [SerializeField] private Animator progressBarAnimator = default;

        #endregion



        #region Methods

        public override void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            base.Deinitialize();
        }


        public void SetObjectActive(bool isActive) =>
            CommonUtility.SetObjectActive(gameObject, isActive);


        public void SetupReceivedRewards(int currentReceivedReward)
        {
            for (int i = 0; i < chestsData.Length; i++)
            {
                CommonUtility.SetObjectActive(chestsData[i].image.gameObject, i >= currentReceivedReward);
            }
        }


        public void SetChestsVisual(Sprite[] sprites)
        {
            if (chestsData.Length != sprites.Length)
            {
                CustomDebug.Log("Chests images count isn't equals to real chests count!");
            }

            for (int i = 0; i < chestsData.Length; i++)
            {
                chestsData[i].image.sprite = sprites[i];
            }
        }


        public void RefreshStage(int currentStage, int stageCount)
        {
            if (stageText != null)
            {
                stageText.text = $"{currentStage}/{stageCount}";
            }
        }


        protected override void OnUpdateProgress(float from, float to, Action<float> onValueChanged = null)
        {
            float beginFillAmount = CalculateNormalizedProgress(from);
            float endFillAmount = CalculateNormalizedProgress(to);

            // Crunch lower
            List<ChestData> chestDataToReach = Array.FindAll(chestsData, e => beginFillAmount <= e.progressTreshholdToBounce &&
                                                                          endFillAmount >= e.progressTreshholdToBounce)
                                                    .ToList();
            onValueChanged += OnChanged;
            base.OnUpdateProgress(from, to, onValueChanged);

            progressBarAnimator.SetTrigger(AnimationKeys.Screen.Show);

            Scheduler.Instance.CallMethodWithDelay(this, () => SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.CHALLENGE_PROGRESSBAR_RISEUP),
                progressBarFillAnimation.delay);

            void OnChanged(float value)
            {
                List<ChestData> chestDataToBounce = chestDataToReach.FindAll(e => value >= e.progressTreshholdToBounce);

                foreach (var d in chestDataToBounce)
                {
                    d.animator.SetTrigger(AnimationKeys.Common.Bounce);
                }

                chestDataToReach.RemoveAll(e => chestDataToBounce.Exists(d => d == e));
            }
        }

        #endregion
    }
}
