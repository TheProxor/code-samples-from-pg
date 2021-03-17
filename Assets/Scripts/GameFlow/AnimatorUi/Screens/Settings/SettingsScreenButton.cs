using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

namespace Drawmasters
{
    [Serializable]
    public class SettingsScreenButton
    {
        #region Nested types

        [Serializable]
        private class Data
        {
            [SerializeField] private Graphic graphic = default;

            [SerializeField] private Color enabledColor = default;
            [SerializeField] private Color disabledColor = default;

            public void SetVisual(bool enabled)
            {
                Color colorToSet = enabled ? enabledColor : disabledColor;
                graphic.color = colorToSet;
            }
        }

        [Serializable]
        private class SpritesData
        {
            [SerializeField] private Image image = default;

            [SerializeField] private Sprite enabledSprite = default;
            [SerializeField] private Sprite disabledSprite = default;

            public void SetVisual(bool isEnabled)
            {
                Sprite sprite = isEnabled ? enabledSprite : disabledSprite;
                image.sprite = sprite;
                image.SetNativeSize();
            }
        }

        #endregion



        #region Fields

        [SerializeField] private Button button = default;

        [SerializeField] private VectorAnimation toDisableAnimation = default;
        [SerializeField] private VectorAnimation toEnableAnimation = default;

        [SerializeField] private Data[] visualData = default;
        [SerializeField] private SpritesData[] spritesData = default;

        [SerializeField] private RectTransform toggleRectTransform = default;

        private Action enableStateCallback;
        private Action disableStateCallback;

        #endregion



        #region Properties

        public bool IsEnabledState { get; private set; }

        #endregion



        #region Methods

        public void Initialize()
        {
            RefreshVisual(true);
            button.onClick.AddListener(Switch);
        }


        public void Deinitialize()
        {
            button.onClick.RemoveListener(Switch);

            DOTween.Kill(this);
        }


        public void SetEnabled(bool enabled)
        {
            bool isImmediately = IsEnabledState == enabled;
            IsEnabledState = enabled;
            RefreshVisual(isImmediately);
        }


        public void AddButtonOnClickCallback(Action callback, bool isEnabledButton)
        {
            if (isEnabledButton)
            {
                enableStateCallback += callback;
            }
            else
            {
                disableStateCallback += callback;
            }
        }


        public void RemoveButtonOnClickCallback(Action callback, bool isEnabledButton)
        {
            if (isEnabledButton)
            {
                enableStateCallback -= callback;
            }
            else
            {
                disableStateCallback -= callback;
            }
        }


        private void RefreshVisual(bool isImmediately)
        {
            foreach (var data in visualData)
            {
                data.SetVisual(IsEnabledState);
            }

            foreach (var spriteData in spritesData)
            {
                spriteData.SetVisual(IsEnabledState);
            }

            VectorAnimation toggleAnimation = IsEnabledState ? toEnableAnimation : toDisableAnimation;

            DOTween.Kill(this, true);

            if (isImmediately)
            {
                toggleRectTransform.localPosition = toggleAnimation.endValue;
            }
            else
            {
                toggleAnimation.Play(position => toggleRectTransform.localPosition = position, this);
            }

        }

        #endregion



        #region Events handlers

        private void Switch()
        {
            Action callback = IsEnabledState ? enableStateCallback : disableStateCallback;

            callback?.Invoke();
        }

        #endregion
    }
}
