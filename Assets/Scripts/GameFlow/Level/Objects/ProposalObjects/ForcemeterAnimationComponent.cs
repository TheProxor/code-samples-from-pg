using Drawmasters.Ui;
using System;
using DG.Tweening;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Spine;
using Animation = Spine.Animation;
using Event = Spine.Event;


namespace Drawmasters.Levels
{
    public class ForcemeterAnimationComponent : ForcemeterComponent
    {
        #region Fields

        public static Action<int> OnProgressStartFill;
        public static Action<int> OnProgressFinishFill;

        public static Action<int> OnShouldApplyReward;

        private const int AnimationIndex = 0;
        private const int FillAnimationIndex = 1;
        private const int ElementsAnimationIndex = 2;

        private ForceMeterUiSettings settings;

        private TrackEntry fillProgressTracker;
        private int currentIterationIndex;

        #endregion



        #region Methods

        public override void Enable()
        {
            settings = IngameData.Settings.forceMeterUiSettings;

            ForceMeterScreen.OnShouldPlayHitAnimation += ForceMeterScreen_OnShouldPlayHitAnimation;

            forcemeter.SkeletonAnimation.ClearState();

            fillProgressTracker = PlayAnimation(settings.forcemeterFillProgressName, false, FillAnimationIndex);
            fillProgressTracker.TimeScale = 0;
            fillProgressTracker.TrackTime = 0;
        }


        public override void Disable()
        {
            ForceMeterScreen.OnShouldPlayHitAnimation -= ForceMeterScreen_OnShouldPlayHitAnimation;

            DOTween.Kill(this);
        }


        private TrackEntry PlayAnimation(string animationName, bool isLoop, int animationIndex = AnimationIndex)
        {
            Animation animation = forcemeter.SkeletonAnimation.Skeleton.Data.FindAnimation(animationName);

            if (animation == null)
            {
                CustomDebug.Log($"Not found anim data. Name = {animationName}");
            }

            return forcemeter.SkeletonAnimation.AnimationState.SetAnimation(animationIndex, animationName, isLoop);
        }


        private void FillForcemeter(int iterationIndex)
        {
            NumberAnimation factorAnimation = settings.FindTrackTimeAnimation(iterationIndex);

            factorAnimation.Play((value) => fillProgressTracker.TrackTime = value, this, () =>
            {
                PlayReceiveAnimation(iterationIndex);
                OnShouldApplyReward?.Invoke(iterationIndex);

                OnProgressFinishFill?.Invoke(iterationIndex);
            });

            OnProgressStartFill?.Invoke(iterationIndex);
        }


        private void FillAll(int iterationIndex)
        {
            OnProgressStartFill?.Invoke(iterationIndex);

            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < settings.StageAnimationDataLength; i++)
            {
                NumberAnimation factorAnimation = settings.FindTrackTimeAnimation(i);

                int savedIndex = i;
                sequence.Append(factorAnimation.Play((value) => fillProgressTracker.TrackTime = value, this, () =>
                {
                    PlayReceiveAnimation(savedIndex);
                    OnShouldApplyReward?.Invoke(savedIndex);
                }));
            }

            sequence.SetId(this);
            sequence.OnComplete(() => OnProgressFinishFill?.Invoke(settings.StageAnimationDataLength - 1));
        }


        private void PlayReceiveAnimation(int iterationIndex)
        {
            string animationName = settings.FindElementAnimation(iterationIndex);
            PlayAnimation(animationName, false, ElementsAnimationIndex);

            forcemeter.GetRewardElement(iterationIndex).PlayReceiveAnimation();
        }

        #endregion



        #region Events handlers

        private void ForceMeterScreen_OnShouldPlayHitAnimation(int iterationIndex)
        {
            currentIterationIndex = iterationIndex;

            string animationName = settings.FindForcemeterObjectName(iterationIndex);
            TrackEntry tracker = PlayAnimation(animationName, false);
            tracker.Event += Tracker_Event;
        }


        private void Tracker_Event(TrackEntry trackEntry, Event e)
        {
            trackEntry.Event -= Tracker_Event;

            string enableAttachEvent = IngameData.Settings.forceMeterUiSettings.forcemeterFillProgressEvent;

            if (e.ToString().Equals(enableAttachEvent))
            {
                bool isRewardSceneData = GameServices.Instance.LevelEnvironment.Context.IsProposalSceneFromRewardData;
                if (isRewardSceneData)
                {
                    FillAll(currentIterationIndex);
                }
                else
                {
                    FillForcemeter(currentIterationIndex);
                }
            }
        }

        #endregion
    }
}
