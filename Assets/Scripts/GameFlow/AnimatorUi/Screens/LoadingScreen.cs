using System;
using DG.Tweening;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class LoadingScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] private RectTransform rotateParentTransform = default;
        [SerializeField] private float rotateDuration = default;

        #endregion



        #region Overrided properties

        public override ScreenType ScreenType => ScreenType.LoadingScreen;

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null, 
            Action<AnimatorView> onHideEndCallback = null, 
            Action<AnimatorView> onShowBeginCallback = null, 
            Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            rotateParentTransform
                .DORotate(new Vector3(0.0f, 0.0f, 360.0f), rotateDuration, RotateMode.FastBeyond360)
                .SetId(this)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);
        }

        public override void Deinitialize()
        {
            DOTween.Kill(this);

            base.Deinitialize();
        }


        public override void InitializeButtons() { }

        public override void DeinitializeButtons() { }

        #endregion
    }
}
