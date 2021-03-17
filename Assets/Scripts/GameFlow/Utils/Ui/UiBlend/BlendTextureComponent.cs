using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


namespace Drawmasters.Utils.Ui
{
    public class BlendTextureComponent : IInitializable, IDeinitializable
    {
        #region Fields

        private const string MaterialBlendProperty = "_Blend";
        private const string MaterialfirstTextureProperty = "_FirstTex";
        private const string MaterialSecondTextureProperty = "_SecTex";

        private readonly BlendImage imageToBlend;
        private readonly Material savedMaterialInstance;

        private FactorAnimation materialBlendAnimation;

        private ColorAnimation colorChangeAnimation;
        private Graphic[] colorChangeGraphic;

        #endregion



        #region Class lifecycle

        public BlendTextureComponent(BlendImage _imageToBlend, FactorAnimation _materialBlendAnimation)
        {
            imageToBlend = _imageToBlend;
            savedMaterialInstance = new Material(imageToBlend.material);
            imageToBlend.material = savedMaterialInstance;

            colorChangeGraphic = Array.Empty<Graphic>();

            colorChangeAnimation = new ColorAnimation();
            materialBlendAnimation = new FactorAnimation();

            SetupBlendAnimation(_materialBlendAnimation);
        }


        public BlendTextureComponent(BlendImage _imageToBlend, FactorAnimation _materialBlendAnimation, Texture _first, Texture _second) :
            this(_imageToBlend, _materialBlendAnimation)
        {
            SetupTextures(_first, _second);
        }

        #endregion



        #region Methods

        public void Initialize()
        {
            SetBlendFactor(default);
        }


        public void Deinitialize()
        {
            DOTween.Kill(this);
        }


        public void SetupBlendAnimation(FactorAnimation _materialBlendAnimation) =>
            materialBlendAnimation = _materialBlendAnimation;
        


        public void SetupTextures(Texture first, Texture second)
        {
            SetupFirstTexture(first);
            SetupSecondTexture(second);
        }


        public void SetupFirstTexture(Texture first) =>
            savedMaterialInstance.SetTexture(MaterialfirstTextureProperty, first);


        public void SetupSecondTexture(Texture second) =>
            savedMaterialInstance.SetTexture(MaterialSecondTextureProperty, second);


        public void SetupGraphicsColor(ColorAnimation _colorChangeAnimation, params Graphic[] _colorChangeGraphic)
        {
            colorChangeGraphic = _colorChangeGraphic;
            colorChangeAnimation = _colorChangeAnimation;
        }


        public void BlendToSecond(bool isImmediately = false)
        {
            if (isImmediately)
            {
                SetGraphicsColor(colorChangeAnimation.endValue);
                SetBlendFactor(materialBlendAnimation.endValue);
            }
            else
            {
                DOTween.Kill(this);

                materialBlendAnimation.Play(SetBlendFactor, this);
                colorChangeAnimation.Play(SetGraphicsColor, this);
            }
        }


        public void BlendToFirst(bool isImmediately = false)
        {
            if (isImmediately)
            {
                SetBlendFactor(materialBlendAnimation.beginValue);
                SetGraphicsColor(colorChangeAnimation.beginValue);
            }
            else
            {
                DOTween.Kill(this);

                materialBlendAnimation.Play(SetBlendFactor, this, isReversed: true);
                colorChangeAnimation.Play(SetGraphicsColor, this, isReversed: true);
            }
        }


        private void SetBlendFactor(float value)
        {
            imageToBlend.blendFactor = value;
            savedMaterialInstance.SetFloat(MaterialBlendProperty, value);
            imageToBlend.SetMaterialDirty();
        }


        private void SetGraphicsColor(Color color)
        {
            foreach (var gr in colorChangeGraphic)
            {
                gr.color = color;
            }
        }

        #endregion
    }
}
