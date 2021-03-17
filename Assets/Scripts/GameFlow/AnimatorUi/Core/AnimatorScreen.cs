using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Ui
{    
    public abstract class AnimatorScreen : AnimatorView
    {
        #region Fields

        public static event Action<ScreenType> OnScreenShow;
        public static event Action<ScreenType> OnScreenHide;

        [SerializeField] private List<Canvas> overrideLayerOrderCanvases = default;

        private readonly Dictionary<Canvas, int> initialOverrideCanvasesOrder = new Dictionary<Canvas, int>();

        protected Canvas mainCanvas;
        
        #endregion



        #region Properties

        public abstract ScreenType ScreenType { get; }
        
        public override ViewType Type => ViewType.Screen;

        protected override string ShowKey => AnimationKeys.Screen.Show;

        protected override string HideKey => AnimationKeys.Screen.Hide;

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null, 
                                        Action<AnimatorView> onHideEndCallback = null, 
                                        Action<AnimatorView> onShowBeginCallback = null, 
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, 
                            onHideEndCallback, 
                            onShowBeginCallback,
                            onHideBeginCallback);

            mainCanvas = GetComponent<Canvas>();
            if (mainCanvas == null)
            {
                CustomDebug.LogError($"Cannot find canvas. Screen type : {ScreenType}");
                return;
            }
            mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            mainCanvas.worldCamera = UiCamera.Instance.Camera;
            
            foreach (var canvas in overrideLayerOrderCanvases)
            {
                initialOverrideCanvasesOrder.Add(canvas, canvas.sortingOrder);
            }
        }


        public override void Show()
        {
            base.Show();

            InitializeButtons();
            OnScreenShow?.Invoke(ScreenType);
        }


        public override void Hide()
        {
            OnScreenHide?.Invoke(ScreenType);
            DeinitializeButtons();

            base.Hide();
        }


        public override void HideImmediately()
        {
            OnScreenHide?.Invoke(ScreenType);

            base.HideImmediately();
        }

        public abstract void DeinitializeButtons();
        public abstract void InitializeButtons();

        #endregion



        #region Abstract implemention

        public override void SetVisualOrderSettings()
        {
            // transform.position = transform.position.SetZ(ZPosition);
            mainCanvas.sortingOrder = SortingOrder;

            overrideLayerOrderCanvases.ForEach((item) => item.sortingOrder = initialOverrideCanvasesOrder[item] + SortingOrder);
        }


        public override void ResetVisualOrderSettings()
        {
            overrideLayerOrderCanvases.ForEach((item) => item.sortingOrder = initialOverrideCanvasesOrder[item] - SortingOrder);
        }


        protected override void InitPosition()
        {
            transform.localScale = Vector3.one;

            Rect.offsetMin = Vector2.zero;
            Rect.offsetMax = Vector2.zero;

            foreach (var m in monobrowOffsets)
            {
                if (m != null)
                {
                    m.ChangeOffset();
                }
            }
        }

        #endregion
    }
}
