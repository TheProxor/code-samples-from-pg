using System;
using System.Collections.Generic;
using Drawmasters.Proposal;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class UiRewardReceiveScreen : AnimatorScreen
    {
        #region Fields

        [Header("Single skin")]
        [SerializeField] private UiRewardReceiveScreenSkinBehaviour.Data skinBehaviourData = default;
        [Header("Several rewards")]
        [SerializeField] private UiRewardReceiveScreenSeveralBehaviour.Data severalRewardBehaviourData = default;

        private readonly Dictionary<UiRewardReceiveScreenState, IUiRewardReceiveBehaviour> behaviours = new Dictionary<UiRewardReceiveScreenState, IUiRewardReceiveBehaviour>();

        private IUiRewardReceiveBehaviour currentBehaviour;

        #endregion



        #region Overrided properties

        public override ScreenType ScreenType =>
            ScreenType.SpinReward;

        #endregion



        #region Override Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            behaviours.Add(UiRewardReceiveScreenState.SkinReward, new UiRewardReceiveScreenSkinBehaviour(skinBehaviourData, this));
            behaviours.Add(UiRewardReceiveScreenState.SeveralReward, new UiRewardReceiveScreenSeveralBehaviour(severalRewardBehaviourData, this));
        }


        public override void Deinitialize()
        {
            foreach (var behaviour in behaviours.Values)
            {
                behaviour.Deinitialize();
            }

            behaviours.Clear();

            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            foreach (var behaviour in behaviours.Values)
            {
                behaviour.InitializeButtons();
            }
        }


        public override void DeinitializeButtons()
        {
            foreach (var behaviour in behaviours.Values)
            {
                behaviour.DeinitializeButtons();
            }
        }

        #endregion



        #region Methods

        public void SetupFxKey(string fxKey)
        {
            foreach (var b in behaviours)
            {
                b.Value.SetupFxKey(fxKey);
            }
        }


        // Unity animator event
        private void StopEffect()
        {
            foreach (var b in behaviours)
            {
                b.Value.StopFx();
            }
        }


        public void SetupReward(RewardData rewardData)
        {
            SetBehaviour(UiRewardReceiveScreenState.SkinReward);
            currentBehaviour.SetupReward(rewardData);
        }


        public void SetupReward(RewardData[] rewardData)
        {
            SetBehaviour(UiRewardReceiveScreenState.SeveralReward);
            currentBehaviour.SetupReward(rewardData);
        }


        private void SetBehaviour(UiRewardReceiveScreenState screenState)
        {
            foreach (var b in behaviours)
            {
                b.Value.Disable();
            }

            if (behaviours.TryGetValue(screenState, out IUiRewardReceiveBehaviour uiBehaviour))
            {
                currentBehaviour = uiBehaviour;
                uiBehaviour.Enable();
            }
        }
        
        #endregion
    }
}
