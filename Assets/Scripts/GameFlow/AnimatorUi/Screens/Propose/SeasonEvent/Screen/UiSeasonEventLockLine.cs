using System;
using Drawmasters.Proposal;
using Drawmasters.Effects;
using Modules.Sound;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;


namespace Drawmasters.Ui
{
    public class UiSeasonEventLockLine : MonoBehaviour
    {
        #region Fields

        [SerializeField] private RectTransform[] lockLineRoots = default;
        [SerializeField] private RectTransform lockBackground = default;
        [SerializeField] private float lockBackgroundAdditionalHeight = default;

        [SerializeField] private Animator animator = default;
        [SerializeField] private SkeletonGraphic lockSkeletonAnimation = default;

        [Header("Fx")]
        [SerializeField] private Transform fxRoot = default;

        private SeasonEventVisualSettings settings;

        private float elementsHeight;
        private float topOffset;

        private bool isFxEnabled;

        #endregion



        #region Properties

        public RectTransform IconLockRoot => lockSkeletonAnimation.rectTransform;

        #endregion



        #region Methods

        public void SetFxEnabled(bool enabled) =>
            isFxEnabled = enabled;


        public void SetupLockBackgroundHeight(float value)
        {
            float targetHeight = Math.Max(value, Screen.height); // to cover up empty space if reward count aren't too much
            targetHeight += lockBackgroundAdditionalHeight;

            lockBackground.sizeDelta = lockBackground.sizeDelta.SetY(targetHeight);
        }


        public void Initialize(float _elementsHeight, float _topOffset = 0.0f)
        {
            settings = IngameData.Settings.seasonEvent.seasonEventVisualSettings;
            elementsHeight = _elementsHeight;
            topOffset = _topOffset;

            SetFxEnabled(true);
        }


        public void Deinitialize()
        {
            DOTween.Kill(this);
        }


        public void SetPosition(float targetPositionY, bool isImmediately, string showAnimationKey = AnimationKeys.SeasonEvent.ShowLockLine, string hideAnimationKey = AnimationKeys.SeasonEvent.HideLockLine)
        {
            if (isImmediately)
            {
                animator.SetTrigger(AnimationKeys.SeasonEvent.ForceShowLockLine);

                foreach (var lockLineRoot in lockLineRoots)
                {
                    lockLineRoot.anchoredPosition = lockLineRoot.anchoredPosition.SetY(targetPositionY);
                }
            }
            else
            {
                DOTween.Complete(this);
                animator.SetTrigger(hideAnimationKey);

                for (int i = 0; i < lockLineRoots.Length; i++)
                {
                    bool isShortHide = hideAnimationKey.Equals(AnimationKeys.SeasonEvent.ShortHideLockLine, StringComparison.Ordinal);
                    settings.lockLineMoveAnimation.SetupDelay(settings.GetLockLineMoveDelayAfterAnimation(isShortHide));
                    settings.lockLineMoveAnimation.SetupBeginValue(lockLineRoots[i].anchoredPosition);
                    settings.lockLineMoveAnimation.SetupEndValue(lockLineRoots[i].anchoredPosition.SetY(targetPositionY));

                    RectTransform transformToMove = lockLineRoots[i];
                    
                    var tween = settings.lockLineMoveAnimation.Play((value) => transformToMove.anchoredPosition = value, this,
                        () => animator.SetTrigger(showAnimationKey));

                    tween.OnStart(() => PlaySfx(AudioKeys.Ingame.CHALLENGE_PROGRESSBAR_VERTICAL));
                }
            }
        }


        public void SetPositionByIndex(int levelRewardelementIndex, bool isImmediately)
        {
            float targetPositionY = CalculateTargetPositionY(levelRewardelementIndex);

            SetPosition(targetPositionY, isImmediately);
        }


        public float CalculateTargetPositionY(int levelRewardelementIndex)
        {
            // TODO: change logic. hotfix to hide line on tutorial
            float offset = levelRewardelementIndex == 0 ? -topOffset * 1.2f : topOffset;
            float result = levelRewardelementIndex * elementsHeight + offset;

            return result;
        }


        // Unity animator
        private void PlayAnimation(string anim) =>
            lockSkeletonAnimation.AnimationState.SetAnimation(0, anim, false);


        // Unity animator
        private void PlayUnlockFx() =>
            PlayFx(EffectKeys.FxGUISeasonPassLineLockTrailShine);


        // Unity animator
        private void PlayFx(string fxKey)
        {
            if (isFxEnabled)
            {
                EffectManager.Instance.PlaySystemOnce(fxKey, fxRoot.position, fxRoot.rotation, fxRoot);
            }
        }


        // Unity animator
        private void PlaySfx(string key) =>
            SoundManager.Instance.PlayOneShot(key);

        #endregion
    }
}
