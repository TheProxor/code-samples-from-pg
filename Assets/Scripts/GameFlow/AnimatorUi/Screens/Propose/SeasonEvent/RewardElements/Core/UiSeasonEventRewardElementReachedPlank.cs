using TMPro;
using Drawmasters.Utils.Ui;
using UnityEngine;
using System;
using I2.Loc;
using State  = Drawmasters.Proposal.UiSeasonEventRewardElement.State;


namespace Drawmasters.Proposal
{
    public class UiSeasonEventRewardElementReachedPlank : MonoBehaviour
    {
        #region Nested types

        [Serializable]
        private class VisualData
        {
            public State[] states = default;
            public GameObject[] showRoots = default;
        }

        #endregion



        #region Fields

        [SerializeField] private CanvasGroup canvasGroup = default;
        [SerializeField] private BlendImage icon = default;
        [SerializeField] private Localize text = default;

        [SerializeField] private VisualData[] visualData = default;

        private SeasonEventVisualSettings settings;

        #endregion



        #region Properties

        public CanvasGroup CanvasGroup => canvasGroup;

        #endregion



        #region Methods

        public void Initialize()
        {
            settings = IngameData.Settings.seasonEvent.seasonEventVisualSettings;
            text.SetTerm(settings.plankLocalizationKey);

            icon.CreateTextureComponent(settings.materialBlendAnimation, settings.plankGraphicColorAnimation);
            icon.BlendTextureComponent.Initialize();
        }


        public void Deinitialize()
        {
            icon.BlendTextureComponent.Deinitialize();
        }


        public void SetState(State state, bool isImmediately, bool wasStateChanged)
        {
            if (isImmediately || wasStateChanged)
            {
                foreach (var data in visualData)
                {
                    bool isCurrentState = Array.Exists(data.states, s => s == state);
                    CommonUtility.SetObjectsActive(data.showRoots, false);
                }

                VisualData currentVisualData = Array.Find(visualData, d => Array.Exists(d.states, s => s == state));
                if (currentVisualData != null)
                {
                    CommonUtility.SetObjectsActive(currentVisualData.showRoots, true);
                }

                bool isReached = state == State.ReadyToClaim ||
                                state == State.CanClaimForAds;

                if (isReached)
                {
                    icon.BlendTextureComponent.BlendToFirst(isImmediately);
                }
                else
                {
                    icon.BlendTextureComponent.BlendToSecond(isImmediately);
                }
            }
        }

        #endregion
    }
}
