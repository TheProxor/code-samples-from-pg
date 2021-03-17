using System;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Effects;
using Modules.Sound;
using Modules.General;
using Modules.UiKit;


namespace Drawmasters.Ui
{
    public class UiSeasonEventBar : MonoBehaviour, IInitializable, IDeinitializable
    {
        #region Filds

        [SerializeField] private RectTransform scaleLapsNumberRoot = default;
        [SerializeField] private RectTransform shineRoot = default;
        [SerializeField] private Image shineImage = default;
        [SerializeField] private Image levelBackImage = default;
        [SerializeField] private TMP_Text lapsNumberText = default;
        [SerializeField] private TMP_Text totalKeysNumberText = default;
        [SerializeField] private Image barFillImage = default;

        [SerializeField] private Animator animatorBar = default;
        [SerializeField] private AnimationEventsListener animationEvents = default;

        private SeasonEventProposeController controller;

        #endregion



        #region Properties

        public RectTransform LevelBackImage => levelBackImage.rectTransform;

        #endregion



        #region Public methods

        public void Initialize()
        {
            CommonUtility.SetObjectActive(shineRoot.gameObject, false);
            controller = GameServices.Instance.ProposalService.SeasonEventProposeController;
        }


        public void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(this);
        }


        public void Pause()
        {
            DOTween.Pause(this);
            Scheduler.Instance.PauseAllMethodForTarget(this);
        }


        public void Resume()
        {
            DOTween.Play(this);
            Scheduler.Instance.UnpauseAllMethodForTarget(this);
        }


        public void PlayAnimation(float previousCurrencyValue, float currentCurrencyValue, Action onLastLapFilledCallback = default, Action onBarWouldNotFill = default)
        {
            int previousReachIndex = controller.LevelReachIndex(previousCurrencyValue);
            
            bool isMainRewardClaimed = controller.IsRewardClaimed(SeasonEventRewardType.Main, 0);

            previousReachIndex = Mathf.Min(previousReachIndex, controller.MaxLevel - (isMainRewardClaimed ? 2 : 3));

            int currentReachIndex = controller.LevelReachIndex(currentCurrencyValue);
            currentReachIndex = Mathf.Min(currentReachIndex, controller.MaxLevel - (isMainRewardClaimed ? 1 : 3));

            int previousCurrency = Convert.ToInt32(previousCurrencyValue);
            int currentCurrency = Convert.ToInt32(currentCurrencyValue);

            controller.PointsCountOnPreviousShow = currentCurrency;

            Play(previousReachIndex, currentReachIndex);


            void Play(int beginRewardIndex, int endRewardIndex)
            {
                if (beginRewardIndex > endRewardIndex)
                {
                    onBarWouldNotFill?.Invoke();
                    return;
                }

                (int, int) minMax = controller.LevelMinMaxPoints(beginRewardIndex);

                int maxPoints = minMax.Item2 - minMax.Item1;
                if (maxPoints == 0)
                {
                    onBarWouldNotFill?.Invoke();
                    return;
                }
                
                float beginValue = Mathf.Approximately(maxPoints, 0.0f) ? default : (float)(previousCurrency - minMax.Item1) / maxPoints;
                int endPoints = currentCurrency - minMax.Item1;
                endPoints = endPoints > maxPoints ? maxPoints : endPoints;
                float endValue = Mathf.Approximately(maxPoints, 0.0f) ? default : (float)endPoints / maxPoints;

                if (previousCurrency == currentCurrency)
                {
                    barFillImage.fillAmount = endValue;
                    RefreshText(beginRewardIndex + 1, endPoints, maxPoints);
                    RefreshVisual(beginRewardIndex + 1);

                    onBarWouldNotFill?.Invoke();
                }
                else
                {
                    float width = barFillImage.rectTransform.sizeDelta.x;
                    
                    shineImage.color = controller.VisualSettings.shineAlfaAnimation.beginValue;
                    CommonUtility.SetObjectActive(shineRoot.gameObject, endPoints > 0);
                    
                    controller.VisualSettings.fillBarAnimation.beginValue = Mathf.Max(0, beginValue);
                    controller.VisualSettings.fillBarAnimation.endValue = endValue;
                    
                    Tween fillTween = controller.VisualSettings.fillBarAnimation.Play((value) =>
                            {
                                float clampedValue = Mathf.Clamp01(value);
                                barFillImage.fillAmount = clampedValue;
                                shineRoot.anchoredPosition = new Vector2(width * clampedValue, shineRoot.anchoredPosition.y);
                            }, this, () =>
                            {
                                animatorBar.SetTrigger(AnimationKeys.SeasonEvent.BarFill);
                                controller.VisualSettings.laspScaleAnimation.Play((value) => scaleLapsNumberRoot.localScale = value, this, null, true);
                                controller.VisualSettings.shineAlfaAnimation.Play((value) => shineImage.color = value, this,
                                    () =>
                                    {
                                        CommonUtility.SetObjectActive(shineRoot.gameObject, false);
                                        shineImage.color = controller.VisualSettings.shineAlfaAnimation.beginValue;

                                        EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUISeasonPassNumberRhombBarShineLaps, LevelBackImage.transform.position, LevelBackImage.rotation, LevelBackImage);
                                        SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.CHALLENGE_PROGRESSBAR_FULL);

                                        // hotfix logic
                                        bool isOverflow = beginRewardIndex + 1 == controller.MaxLevel - 2 && endRewardIndex + 1 == controller.MaxLevel - 2;
                                        bool wasLastLapFilled = isOverflow || beginRewardIndex + 1 == endRewardIndex;

                                        Play(beginRewardIndex + 1, endRewardIndex);

                                        if (wasLastLapFilled)
                                        {
                                            onLastLapFilledCallback?.Invoke();
                                        }
                                    });
                            });
                    
                    // hotfix to pause visual refresh also. Tween on start has bugs
                    Scheduler.Instance.CallMethodWithDelay(this, () =>
                    {
                        bool wasLastLapFilled = beginRewardIndex == endRewardIndex;
                        if (wasLastLapFilled)
                        {
                            animationEvents.AddListener(() =>
                            {
                                RefreshText(beginRewardIndex + 1, endPoints, maxPoints);
                                RefreshVisual(beginRewardIndex + 1);
                            });
                            animatorBar.SetTrigger(AnimationKeys.SeasonEvent.BarNextLevel);
                        }
                        else
                        {
                            RefreshText(beginRewardIndex + 1, endPoints, maxPoints);
                            RefreshVisual(beginRewardIndex + 1);
                        }

                        SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.CHALLENGE_PROGRESSBAR_RISEUP);
                    }, CommonUtility.OneFrameDelay);
                }
            }
        }

        #endregion



        #region Private methods

        private void RefreshVisual(int nextLevel)
        {
            RefreshIcon();
            RefreshOutlineColor();

            void RefreshIcon()
            {
                bool isOverFlowIcon = nextLevel >= controller.MaxLevel - 2;
                bool isMainRewardIcon = controller.ShouldConsiderMainRewardClaimedForOldUsers ||
                                        controller.IsRewardClaimed(SeasonEventRewardType.Main, 0);
                
                Sprite iconSprite;

                if (isOverFlowIcon)
                {
                    if (isMainRewardIcon)
                    {
                        iconSprite = controller.VisualSettings.barBonusLevelSprite;    
                    }
                    else
                    {
                        PetSkinType petSkinType = controller.IsPetMainReward ? 
                            controller.PetMainRewardType : 
                            PetSkinType.None;
                        
                        iconSprite = IngameData.Settings.pets.uiSettings.FindSmallBarSprite(petSkinType);
                    }
                }
                else
                {
                    iconSprite = controller.VisualSettings.FindLevelElementSprite(nextLevel);
                }

                levelBackImage.sprite = iconSprite;
                levelBackImage.SetNativeSize();
            }


            void RefreshOutlineColor()
            {
                Color outlineColor = controller.VisualSettings.FindLevelElementOutlineColor(nextLevel);
                lapsNumberText.outlineColor = outlineColor;
                Material savedMaterial = lapsNumberText.fontMaterial;
                lapsNumberText.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, lapsNumberText.outlineColor);
                lapsNumberText.fontMaterial = new Material(savedMaterial);
            }
        }


        private void RefreshText(int nextLevel, int point, int maxPoint)
        {
            bool isOverFlow = nextLevel >= controller.MaxLevel - 2;

            if (lapsNumberText != null)
            {
                lapsNumberText.text = isOverFlow ? string.Empty : nextLevel.ToString();
            }
            
            if (totalKeysNumberText != null)
            {
                totalKeysNumberText.text = $"{point}/{maxPoint}";
            }
        }

        #endregion
    }
}