using System;
using DG.Tweening;
using Drawmasters.Proposal;
using UnityEngine;
using Drawmasters.Utils;
using Spine.Unity;


namespace Drawmasters.Ui
{
    public class MonopolyCharacter : MonoBehaviour, IInitializable, IDeinitializable
    {
        #region Nested types

        [Flags]
        public enum JumpDirection
        {
            None        = 0,
            Horizontal  = 1,
            Vectical    = 2
        }

        #endregion



        #region Fields

        private const float EmotionAnimationDelay = 4.0f;

        private const int MainAnimationIndex = 0;
        private const int EmotionAnimationIndex = 0;

        [SerializeField] private SkeletonGraphic skeletonGraphic = default;

        [SerializeField]
        [SpineAnimation(dataField = "skeletonGraphic")]
        private string idleAnimation = default;

        [SerializeField]
        [SpineAnimation(dataField = "skeletonGraphic")]
        private string jumpStartAnimation = default;
        [SerializeField]
        [SpineAnimation(dataField = "skeletonGraphic")]
        private string jumpFinishAnimation = default;

        [SerializeField]
        [SpineAnimation(dataField = "skeletonGraphic")]
        private string[] idleEmotionAnimations = default;

        private MonopolyVisualSettings settings;
        private VectorAnimation moveAnimation;

        private Timer emotionAnimationTimer;

        #endregion



        #region IInitializable

        public void Initialize()
        {
            settings = IngameData.Settings.monopoly.visualSettings;
            emotionAnimationTimer = new Timer();
            emotionAnimationTimer.Start();

            skeletonGraphic.Initialize(true);

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            emotionAnimationTimer.Stop();

            DOTween.Kill(this);
        }

        #endregion



        #region Methods

        public void MoveToPosition(Vector3 position, JumpDirection jumpDirection, Action callback, bool isImmediately)
        {
            DOTween.Complete(this);

            RectTransform rectTransform = transform as RectTransform;

            if (isImmediately)
            {
                rectTransform.anchoredPosition3D = position;
                callback?.Invoke();
            }
            else
            {
                emotionAnimationTimer.Stop();

                if (moveAnimation == null)
                {
                    moveAnimation = new VectorAnimation();
                    moveAnimation.SetupData(IngameData.Settings.monopoly.visualSettings.characterMoveAnimation);
                }

                AnimationCurve curveX = settings.FindCharacterCurveX(jumpDirection);
                AnimationCurve curveY = settings.FindCharacterCurveY(jumpDirection);

                Vector3 startPos = rectTransform.anchoredPosition3D;
                Vector3 endPos = position;

                DOTween.Sequence()
                    .Join(DOTween.To(() => 0f, y =>
                    {
                        float positionY = jumpDirection == JumpDirection.Horizontal ?
                            startPos.y + Mathf.Abs(startPos.y) * curveY.Evaluate(y) : 
                            Mathf.LerpUnclamped(startPos.y, endPos.y, curveY.Evaluate(y));

                        rectTransform.anchoredPosition3D = rectTransform.anchoredPosition3D.SetY(positionY);
                    }, 1f, moveAnimation.duration))
                    .Join(rectTransform.DOAnchorPosX(endPos.x, moveAnimation.duration)
                        .SetEase(curveX)
                        .SetId(this))
                    .OnComplete(() =>
                    {
                        emotionAnimationTimer.Start();

                        callback?.Invoke();
                    })
                    .SetId(this)
                    .Play();
            }
        }


        public void PlayStartJumpAnimation() =>
            skeletonGraphic.AnimationState.SetAnimation(MainAnimationIndex, jumpStartAnimation, false);


        public void PlayFinishJumpAnimation()
        {
            var track = skeletonGraphic.AnimationState.SetAnimation(MainAnimationIndex, jumpFinishAnimation, false);

            Spine.AnimationState.TrackEntryDelegate trCallback = null;
            trCallback = (v) =>
            {
                PlayIdleAnimation();
                v.Complete -= trCallback;
            };

            track.Complete += trCallback;
        }


        private void PlayIdleAnimation()
        {
            skeletonGraphic.AnimationState.SetAnimation(MainAnimationIndex, idleAnimation, true);
        }


        private void PlayIdleEmotionAnimation()
        {
            skeletonGraphic.AnimationState.SetAnimation(EmotionAnimationIndex, idleEmotionAnimations.RandomObject(), false);
        }


        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (emotionAnimationTimer.RoundedValue > EmotionAnimationDelay)
            {
                PlayIdleEmotionAnimation();

                emotionAnimationTimer.Reset();
            }
        }

        #endregion
    }
}
