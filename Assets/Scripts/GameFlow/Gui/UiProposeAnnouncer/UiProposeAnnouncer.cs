using System;
using UnityEngine;
using Modules.General;
using Drawmasters.Effects;
using TMPro;
using DG.Tweening;
using Modules.Sound;


namespace Drawmasters.Ui
{
    public class UiProposeAnnouncer : MonoBehaviour
    {
        #region Fields

        public event Action OnAnimationFinished;
        public event Action OnShouldShowProgressBar;

        [SerializeField] private TMP_Text labelText = default;

        [SerializeField] private CanvasGroup canvasGroup = default;

        [Header("Animation")]
        [SerializeField] private Animator animator = default;
        [SerializeField] private VectorAnimation moveAnimation = default;

        [Header("Fxs")]
        [SerializeField] private Transform fxRoot = default;

        private Transform finishRoot;
        private string fxKey;

        #endregion


        
#warning Why it's in Unity Lifecycle rather than IDeinitialize? TO DMITRY S
        #region Unity Lifecycle

        private void OnEnable() =>
            Initialize();


        private void OnDisable() =>
            Deinitialize();

        #endregion



        #region Methods

        public void SetupFxKey(string _fxKey) =>
            fxKey = _fxKey;


        public virtual void Initialize() =>
            canvasGroup.alpha = 0;


        public virtual void Deinitialize()
        {
            canvasGroup.alpha = 0;
            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public void Show(Transform _root, Transform _target, string text)
        {
            if (_target == null || _root == null)
            {
                CustomDebug.Log("Ui Propose Announcer <b>target or _root</b> is NULL!");
                return;
            }

            transform.position = _root.position;
            finishRoot = _target;

            canvasGroup.alpha = 1.0f;
            labelText.text = text;
             
            animator.SetTrigger("Show");
        }


        // Called from unity animator
        private void StartMoveIntoTarget()
        {
            moveAnimation.beginValue = transform.position;
            moveAnimation.endValue = finishRoot.position;

            moveAnimation.Play((value) => transform.position = value, this, Deinitialize);
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SKULL_COLLECT);
        }


        // Called from unity animator
        private void CreateEffect()
        {
            EffectHandler effectHandler = EffectManager.Instance.PlaySystemOnce(fxKey,
                                                                                parent: fxRoot,
                                                                                transformMode: TransformMode.Local);

            ParticleSystemForceField forceField = effectHandler.GetComponentInChildren<ParticleSystemForceField>();

            if (forceField == null)
            {
                CustomDebug.Log($"{this}. Vfx with the key = {fxKey} has no {nameof(ParticleSystemForceField)}");
                Deinitialize();
                return;
            }

            forceField.transform.position = finishRoot.position;
        }


        // Called from unity animator
        private void AnimationFinished() =>
            OnAnimationFinished?.Invoke();


        // Called from unity animator
        private void ShouldShowProgressBar() =>
            OnShouldShowProgressBar?.Invoke();

        
        #endregion
    }
}
