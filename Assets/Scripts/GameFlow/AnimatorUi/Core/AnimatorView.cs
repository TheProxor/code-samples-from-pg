using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Ui
{
    [RequireComponent(typeof(Animator))]
    public abstract class AnimatorView : MonoBehaviour, IDeinitializable, IView
    {
        #region Fields

        public event Action<AnimatorView> OnHideEnd;
        public event Action<AnimatorView> OnHideBegin;

        public event Action<AnimatorView> OnShowBegin;
        public event Action<AnimatorView> OnShowEnd;

        [SerializeField] protected List<IphoneXOffset> monobrowOffsets = default;

        protected Animator mainAnimator;

        private List<ShowState> showStates;
        private List<HideState> hideStates;

        private Action<AnimatorView> showEndCallback;
        private Action<AnimatorView> hideEndCallback;

        private Action<AnimatorView> showBeginCallback;
        private Action<AnimatorView> hideBeginCallback;

        private RectTransform rect;

        #endregion


        #region Properties

        protected abstract string ShowKey { get; }

        protected abstract string HideKey { get; }

        protected virtual string IdleBeforeHideKey { get; }

        protected virtual string IdleAfterShowKey { get; }

        protected RectTransform Rect
        {
            get
            {
                if (rect == null)
                {
                    rect = GetComponent<RectTransform>();
                }

                return rect;
            }
        }

        #endregion



        #region IView

        public abstract ViewType Type { get; }

        public int SortingOrder { get; set; }

        public float ZPosition { get; set; }

        public abstract void SetVisualOrderSettings();

        public abstract void ResetVisualOrderSettings();

        #endregion



        #region Methods

        public virtual void Show(Action<AnimatorView> onShowEnd, Action<AnimatorView> onShowBegin)
        {
            showEndCallback += onShowEnd;
            showBeginCallback += onShowBegin;

            Show();
        }

        public virtual void Show()
        {
            CommonUtility.SetObjectActive(gameObject, true);
            SubscribeEvents();

            OnShowBegin?.Invoke(this);

            mainAnimator.enabled = true;
            SetInputStateEnabled(false);

            mainAnimator.SetTrigger(ShowKey);
        }


        public void Hide(Action<AnimatorView> onHideEnd, Action<AnimatorView> onHideBegin)
        {
            hideEndCallback += onHideEnd;
            hideBeginCallback += onHideBegin;

            Hide();
        }


        public virtual void Hide()
        {
            OnHideBegin?.Invoke(this);

            SetInputStateEnabled(false);

            mainAnimator.SetTrigger(HideKey);

            if (!string.IsNullOrEmpty(IdleBeforeHideKey))
            {
                mainAnimator.Play(IdleBeforeHideKey);
            }
        }


        public void HideImmediately(Action<AnimatorView> callback)
        {
            hideEndCallback += callback;

            HideImmediately();
        }


        public virtual void HideImmediately()
        {
            OnHideBegin?.Invoke(this);
            
            hideBeginCallback?.Invoke(this);
            hideBeginCallback = null;

            FinishHide();
        }


        public void AddShowBeginCallback(Action<AnimatorView> callback) =>
            showBeginCallback += callback;

        //TODO HACK because of callback execution order
        public void AddOnHiddenCallback(Action<AnimatorView> callback) =>
            hideEndCallback += callback;

        private void FinishHide()
        {
            SetInputStateEnabled(true);

            UnsubscribeEvents();

            CommonUtility.SetObjectActive(gameObject, false);

            OnHideEnd?.Invoke(this);

            hideEndCallback?.Invoke(this);
            hideEndCallback = null;
        }

        #endregion



        #region IInitializable

        public virtual void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                       Action<AnimatorView> onHideEndCallback = null,
                                       Action<AnimatorView> onShowBeginCallback = null,
                                       Action<AnimatorView> onHideBeginCallback = null)
        {
            CommonUtility.SetObjectActive(gameObject, false);

            mainAnimator = GetComponent<Animator>();
            mainAnimator.enabled = false;

            showEndCallback = onShowEndCallback;
            hideEndCallback = onHideEndCallback;
            showBeginCallback = onShowBeginCallback;
            hideBeginCallback = onHideBeginCallback;
            
            InitPosition();
        }

        #endregion



        #region IDeinitializable

        public virtual void Deinitialize()
        {
            UnsubscribeEvents();
        }

        #endregion



        #region Private methods

        private void SubscribeEvents()
        {
            showStates = new List<ShowState>(mainAnimator.GetBehaviours<ShowState>());
            showStates.ForEach(state =>
            {
                state.Initialize(mainAnimator);
                state.OnShowBegin += ShowState_OnShowBegin;
            });


            hideStates = new List<HideState>(mainAnimator.GetBehaviours<HideState>());
            hideStates.ForEach(state =>
            {
                state.Initialize(mainAnimator);
                state.OnHideBegin += HideState_OnHideBegin;
                state.OnHideEnd += HideState_OnHideEnd;
            });
        }


        private void UnsubscribeEvents()
        {
            showStates?.ForEach(state =>
            {
                state.OnShowBegin -= ShowState_OnShowBegin;
            });

            hideStates?.ForEach(state =>
            {
                state.OnHideBegin -= HideState_OnHideBegin;
                state.OnHideEnd -= HideState_OnHideEnd;
            });
        }


        protected abstract void InitPosition();


        private void FinishShow()
        {
            SetInputStateEnabled(true);

            OnShowEnd?.Invoke(this);

            showEndCallback?.Invoke(this);
            showEndCallback = null;
        }

        protected void SetInputStateEnabled(bool isEnabled) => EventSystemController.SetSystemEnabled(isEnabled, this);

        #endregion



        #region Events handlers

        private void ShowState_OnShowBegin()
        {
            showBeginCallback?.Invoke(this);
            showBeginCallback = null;
        }


        //method is invoked by animation event
        private void ShowState_OnShowEnd()
        {
            FinishShow();
        }


        private void HideState_OnHideBegin()
        {
            hideBeginCallback?.Invoke(this);
            hideBeginCallback = null;
        }


        private void HideState_OnHideEnd()
        {
            FinishHide();
        }

        #endregion



        #region Editor

        [Sirenix.OdinInspector.Button]
        private void FillIphoneXOffset()
        {
            monobrowOffsets = new List<IphoneXOffset>();

            var ar = gameObject.GetComponentsInChildren<IphoneXOffset>(true);

            monobrowOffsets.AddRange(ar);
        }

        #endregion
    }
}
