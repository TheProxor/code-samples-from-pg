using System;
using Drawmasters.Effects;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class UiLeagueLeaderBoardScreenInfo : AnimatorScreen
    {
        #region Nested types

        [Serializable]
        private class VisualData
        {
            public LeagueType leagueType = default;
            public RectTransform root = default;
            public TextMeshProUGUI text = default;
            public SkeletonGraphic skeletonGraphic = default;
            public IdleEffect[] idleEffects;
        }

        #endregion
     
        
        
        #region Fields

        private const int enableFontSizeMax = 33;
        private const int disableFontSizeMax = 30;
        
        [SerializeField] private VisualData[] visualData = default;
        [SerializeField] private Button closeButton = default;
        
        [SerializeField] private Vector3 disableScale = default;
        [SerializeField] private Color disableColor1 = default;
        [SerializeField] private Color disableColor2 = default;

        [SerializeField] private Vector3 enableScale = default;
        [SerializeField] private Color enableColor1 = default;
        [SerializeField] private Color enableColor2 = default;
        
        private LeagueProposeController controller;

        #endregion



        #region Overrided properties

        public override ScreenType ScreenType =>
            ScreenType.LeagueInfo;

        #endregion



        #region Overrided methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
            Action<AnimatorView> onHideEndCallback = null,
            Action<AnimatorView> onShowBeginCallback = null,
            Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            controller = GameServices.Instance.ProposalService.LeagueProposeController;
        }

        
        public override void Show()
        {
            base.Show();

            RefreshVisual(controller.LeaderBoard.LeagueType);
        }

        
        public override void DeinitializeButtons()
        {
            closeButton.onClick.RemoveListener(Hide);
        }


        public override void InitializeButtons()
        {
            closeButton.onClick.AddListener(Hide);
        }

        #endregion
        
        
        
        #region Methods
        
        
        public void SetLeague(LeagueType leagueType)
        {
            RefreshVisual(leagueType);
        }


        private void RefreshVisual(LeagueType leagueType)
        {
            foreach (var item in visualData)
            {
                TMP_ColorGradient gradient = ScriptableObject.CreateInstance<TMP_ColorGradient>();
                gradient.colorMode = ColorMode.VerticalGradient;

                if (item.leagueType == leagueType)
                {
                    item.root.localScale = enableScale;
                    gradient.topLeft = enableColor1;
                    gradient.topRight = enableColor1;
                    gradient.bottomLeft = enableColor2;
                    gradient.bottomRight = enableColor2;

                    item.text.fontSizeMax = enableFontSizeMax;

                    foreach (var effect in item.idleEffects)
                    {
                        effect.CreateAndPlayEffect();
                    }
                    
                    SpineUtility.SafeSetAnimation(item.skeletonGraphic,
                        controller.VisualSettings.FindIdleWhiteAnimationKey(item.leagueType), 0, true);
                }
                else
                {
                    item.root.localScale = disableScale;
                    gradient.topLeft = disableColor1;
                    gradient.topRight = disableColor1;
                    gradient.bottomLeft = disableColor2;
                    gradient.bottomRight = disableColor2;

                    item.text.fontSizeMax = disableFontSizeMax;
                    
                    foreach (var effect in item.idleEffects)
                    {
                        effect.StopEffect();
                    }
                    
                    SpineUtility.SafeSetAnimation(item.skeletonGraphic,
                        controller.VisualSettings.FindIdleAnimationKey(item.leagueType), 0, true);
                }
                item.text.colorGradientPreset = gradient;
            }
        }
        
        #endregion
    }
}
