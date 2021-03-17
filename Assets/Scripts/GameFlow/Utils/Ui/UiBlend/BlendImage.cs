using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Utils.Ui
{
    public class BlendImage : Image
    {
        #region Fields

        private const string MaterialBlendProperty = "_Blend";

        [SerializeField] private Graphic[] colorChangeRenderers = default;

        [Range(default, 1.0f)] public float blendFactor = default;

        #endregion



        #region Properties

        public BlendTextureComponent BlendTextureComponent { get; private set; }

        #endregion



        #region Methods

        public void CreateTextureComponent(FactorAnimation blendAnimation, ColorAnimation colorAnimation)
        {
            CreateTextureComponent(blendAnimation);
            BlendTextureComponent.SetupGraphicsColor(colorAnimation, colorChangeRenderers);
        }


        public void CreateTextureComponent(FactorAnimation blendAnimation) =>
            BlendTextureComponent = BlendTextureComponent ?? new BlendTextureComponent(this, blendAnimation);


        public void CreateTextureComponent(FactorAnimation blendAnimation, Texture first, Texture second, ColorAnimation colorAnimation)
        {
            CreateTextureComponent(blendAnimation, first , second);
            BlendTextureComponent.SetupGraphicsColor(colorAnimation, colorChangeRenderers);
        }


        public void CreateTextureComponent(FactorAnimation blendAnimation, Texture first, Texture second) =>
            BlendTextureComponent = BlendTextureComponent ?? new BlendTextureComponent(this, blendAnimation, first, second);


        #endregion


        #region Overrided methods

        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            Material modifiedMaterial = base.GetModifiedMaterial(baseMaterial);

            modifiedMaterial.SetFloat(MaterialBlendProperty, blendFactor);

            return modifiedMaterial;
        }

        #endregion
    }
}
