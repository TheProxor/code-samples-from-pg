using UnityEngine;
using System;
using System.Linq;
using DG.Tweening;
using Drawmasters.Effects;
using Drawmasters.Proposal;
using Drawmasters.Utils;
using UnityEngine.UI;
using TMPro;
using I2.Loc;
using Modules.General;
using ntw.CurvedTextMeshPro;
using Spine.Unity;

namespace Drawmasters.Ui
{
    public class UiLeagueChangeScreen : AnimatorScreen
    {
        #region Helpers

        [Serializable]
        public class VisualData
        {
            public LeagueType type = default;
            public RectTransform transform = default;
            public SkeletonGraphic skeletonGraphic = default;
            public Transform idleFxRoot = default;

            public IdleEffect[] idleEffectHandlers = default;
        }

        #endregion



        #region Fields

        [SerializeField] private Button openButton = default;
        [SerializeField] private RectTransform scrollContentRect = default;
        [SerializeField] private VisualData[] leagueElements = default;
        [SerializeField] private Localize leagueLocalizeText = default;
        [SerializeField] private TMP_Text descriptionText = default;
        [SerializeField] private TextProOnACircle textProOnACircle;

        private LeagueProposeController controller;
        private VectorAnimation moveAnimation;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.LeagueChange;

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null, Action<AnimatorView> onHideEndCallback = null, Action<AnimatorView> onShowBeginCallback = null, Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            LocalizationManager.OnLocalizeEvent += LocalizationManager_OnLocalizeEvent;
        }

        public override void Deinitialize()
        {
            DOTween.Kill(this);
            
            foreach (var element in leagueElements)
            {
                foreach (var effectHandler in element.idleEffectHandlers)
                {
                    effectHandler.StopEffect();        
                }
            }
            
            LocalizationManager.OnLocalizeEvent -= LocalizationManager_OnLocalizeEvent;

            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            openButton.onClick.AddListener(OpenButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            openButton.onClick.RemoveListener(OpenButton_OnClick);
        }


        public void SetupController(LeagueProposeController _controller, LeagueType reachedLeague)
        {
            DeinitializeButtons();
            controller = _controller;
            moveAnimation = controller.VisualSettings.scrollLeagueChangeAnimation;

            LeagueType currentLeagueType = reachedLeague.GetPreviousLeague();
            LeagueType nextLeagueType = reachedLeague;

            RefreshVisual(currentLeagueType);
            
            MoveLayoutElement(scrollContentRect, currentLeagueType, true, () =>
            {
                MoveLayoutElement(scrollContentRect, nextLeagueType, false, () =>
                    {
                        RefreshVisual(nextLeagueType);

                        foreach (var leagueElement in leagueElements)
                        {
                            if (leagueElement.type == nextLeagueType)
                            {
                                SpineUtility.SafeSetAnimation(leagueElement.skeletonGraphic,
                                    controller.VisualSettings.FindIdleWhiteAnimationKey(leagueElement.type), 0, false);

                                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUILeagueChange, 
                                    leagueElement.idleFxRoot.position, 
                                    leagueElement.idleFxRoot.rotation, 
                                    leagueElement.idleFxRoot);

                                foreach (var effectHandler in leagueElement.idleEffectHandlers)
                                {
                                    effectHandler.CreateAndPlayEffect();    
                                }
                            }
                        }
                        
                        InitializeButtons();
                    });
            });
        }


        private void MoveLayoutElement(RectTransform content, LeagueType leagueType, bool isImmediately, Action callback = default)
        {
            Vector2 endPosition = content.anchoredPosition;
            Vector2 startPosition = content.anchoredPosition;

            float minScale = controller.VisualSettings.leagueIconMinScale;
            float maxScale = 1.0f;

            LeagueType previousLeagueType = leagueType.GetPreviousLeague();
            VisualData prevItem = leagueElements.FirstOrDefault(x => x.type == previousLeagueType);

            VisualData nextItem = leagueElements.FirstOrDefault(x => x.type == leagueType);

            if (nextItem != null)
            {
                endPosition = new Vector2(moveAnimation.endValue.x - nextItem.transform.anchoredPosition.x,
                                          moveAnimation.endValue.y - nextItem.transform.anchoredPosition.y);
            }

            if (isImmediately)
            {
                AnimationCallback();
            }
            else
            {
                Vector2 d = endPosition - startPosition;
                
                controller.VisualSettings.scrollLeagueChangeSwipeAnimation.Play((value) =>
                {
                    content.anchoredPosition = startPosition + d * value;

                    float newItemScale = minScale + minScale * value;
                    SafeSetupScale(nextItem, Vector3.one * newItemScale);

                    float prevItemScale = maxScale - minScale * value;
                    SafeSetupScale(prevItem, Vector3.one * prevItemScale);

                }, this, AnimationCallback);
            }

            void AnimationCallback()
            {
                content.anchoredPosition = endPosition;

                foreach (var data in leagueElements)
                {
                    Vector3 targetScale = data == nextItem ? Vector3.one * maxScale : Vector3.one * minScale;
                    SafeSetupScale(data, targetScale);
                }

                SafeSetupScale(nextItem, Vector3.one * maxScale);
                SafeSetupScale(prevItem, Vector3.one * minScale);

                callback?.Invoke();
            }

            void SafeSetupScale(VisualData target, Vector3 value)
            {
                if (target != null && target.transform != null)
                {
                    target.transform.localScale = value;
                }
            }
        }


        private void RefreshVisual(LeagueType type)
        {
            string headerKey = controller.VisualSettings.FindHeaderKey(type);

            leagueLocalizeText.SetTerm(headerKey);

            string descriptionKey = controller.VisualSettings.localizationDescriptionKey;
            string leagueKey = controller.VisualSettings.localizationDescriptionLeaguePrefix + ConvertLeagueTypeName(type);

            string formatDescription = LocalizationManager.GetTermTranslation(descriptionKey);
            string league = LocalizationManager.GetTermTranslation(leagueKey);

            string outText = formatDescription.SafeStringFormat(league);

            descriptionText.text = outText;

            textProOnACircle.enabled = false;
            Scheduler.Instance.CallMethodWithDelay(this, () => textProOnACircle.enabled = true, CommonUtility.OneFrameDelay);
        }


        private string ConvertLeagueTypeName(LeagueType type)
        {
            switch (type)
            {
                case LeagueType.Wooden:
                    return "shooters";
                
                case LeagueType.Bronze:
                    return "rangers";
                
                case LeagueType.Silver:
                    return "snipers";
                
                case LeagueType.Gold:
                    return "masters";
            }

            return string.Empty;
        }
        
        #endregion



        #region Events handlers

        private void OpenButton_OnClick()
        {
            DeinitializeButtons();

            if (controller.IsActive)
            {
                controller.Propose(); 
                controller.MarkForceProposed();
            }
            else
            {
                Hide();
            }
        }


        private void LocalizationManager_OnLocalizeEvent() =>
            RefreshVisual(controller.LeagueReachController.ReachedLeague);

        #endregion
    }
}
