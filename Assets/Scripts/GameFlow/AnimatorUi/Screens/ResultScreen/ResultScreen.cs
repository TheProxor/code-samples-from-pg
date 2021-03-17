using Drawmasters.Levels;
using DG.Tweening;
using System;
using UnityEngine;
using Modules.Sound;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.ServiceUtil;
using Modules.General;


namespace Drawmasters.Ui
{
    public class ResultScreen : AnimatorScreen
    {
        #region Fields

        [Header("Lose state")] 
        [SerializeField] [Required] private ResultLoseBehaviour.Data loseData = default;

        [Header("Pause state")] 
        [SerializeField] [Required] private ResultPauseBehaviour.Data pauseData = default;

        [Header("Can claim skin")]
        [SerializeField] [Required] private ResultClaimSkinBehaviour.Data skinData = default;
        
        [Header("Level complete")]
        [SerializeField] [Required] private ResultCommonCompleteBehaviour.Data levelCompleteData = default;

        [Header("Sound")]
        [SerializeField] private float levelCompleteSoundDelay = default;

        private readonly Dictionary<ResultScreenState, IResultBehaviour> behaviours = new Dictionary<ResultScreenState, IResultBehaviour>();

        protected IResultBehaviour currentBehaviour;

        #endregion



        #region Properties

        public override ScreenType ScreenType => ScreenType.Result;

        protected override string IdleBeforeHideKey => AnimationKeys.ResultScreen.CommonIdleBeforeHide;

        private Guid soundGuid;

        #endregion



        #region Overrided methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBegin = null,
                                        Action<AnimatorView> onHideBegin = null)
        {
            base.Initialize(onShowEndCallback, 
                            onHideEndCallback,
                            onShowBegin,
                            onHideBegin);

            behaviours.Add(ResultScreenState.Lose, new ResultLoseBehaviour(loseData, this));
            behaviours.Add(ResultScreenState.Paused, new ResultPauseBehaviour(pauseData, this));
            behaviours.Add(ResultScreenState.CanClaimSkin, new ResultClaimSkinBehaviour(skinData, this));
            behaviours.Add(ResultScreenState.LevelComplete, new ResultCommonCompleteBehaviour(levelCompleteData, this));
            behaviours.Add(ResultScreenState.BonusComplete, new ResultBonusCompleteBehaviour(levelCompleteData, this));
            behaviours.Add(ResultScreenState.BossComplete, new ResultBossCompleteBehaviour(levelCompleteData, this));
            behaviours.Add(ResultScreenState.LiveopsComplete, new ResultLiveOpsCompleteBehaviour(levelCompleteData, this));
            behaviours.Add(ResultScreenState.LiveopsProposalEnd, new ResultLiveOpsProposalEndCompleteBehaviour(levelCompleteData, this));

            RefreshVisual();
        }


        public override void Deinitialize()
        {
            base.Deinitialize();

            currentBehaviour.Deinitialize();

            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public override void Show()
        {
            base.Show();

            SoundManager soundManager = SoundManager.Instance;
            Scheduler.Instance.CallMethodWithDelay(this, () =>
                                      soundManager.PlaySound(AudioKeys.Ui.POPUP_LEVEL_COMPLETE), levelCompleteSoundDelay);

            soundGuid = soundManager.PlaySound(AudioKeys.Music.MUSIC_LOOP_POPUP_WAITING, isLooping: true);
        }

        public override void DeinitializeButtons() { }

        public override void InitializeButtons() { }

        #endregion



        #region Public methods

        public void LoadModeHideAction()
        {
            currentBehaviour.DeinitializeButtons();


            FromLevelToLevel.PlayTransition(() =>
            {
                SoundManager.Instance.StopSound(soundGuid);

                HideImmediately();
            });
        }


        public void SwitchState(string animationTrigger) =>
            mainAnimator.SetTrigger(animationTrigger);

        #endregion
        
        
        
        #region Private methods
        
        // also invoked by unity animation event
        private void RefreshVisual()
        {
            ILevelEnvironment service = GameServices.Instance.LevelEnvironment;

            LevelProgress levelProgress = service.Progress;
            LevelContext levelContext = service.Context;

            ResultScreenState state = ResultScreenState.None; 
            state = state.DefineState(levelProgress, levelContext);
            
            if (behaviours.TryGetValue(state, out IResultBehaviour resultBehaviour))
            {
                if (currentBehaviour == null)
                {
                    currentBehaviour = resultBehaviour;
                    currentBehaviour.Enable();
                }
                else if (resultBehaviour != null && currentBehaviour != resultBehaviour)
                {
                    currentBehaviour.Disable();
                    currentBehaviour = resultBehaviour;
                    currentBehaviour.Enable();
                }
            }

            // foreach (var b in behaviours.Values)
            // {
            //     if (!b.Equals(currentBehaviour))
            //     {
            //         b.Disable();
            //     }
            // }

            // if (currentBehaviour != null)
            // {
            //     currentBehaviour.Enable();
            // }
        }
        
        #endregion
    }
}
