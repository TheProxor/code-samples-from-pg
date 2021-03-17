using UnityEngine;
using DG.Tweening;


namespace Drawmasters.Levels
{
    public abstract class PhysicalObjectHighlightComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        private static class ShaderProperties
        {
            public const string _MainTex = "_MainTex";
            public const string _OutlineWidth = "_OutlineWidth";
            public const string _OutlineColor = "_OutlineColor";
            public const string _ThresholdEnd = "_ThresholdEnd"; 
        }

        private MaterialPropertyBlock block;
        private MaterialPropertyBlock dblock;

        private SpriteRenderer outlineSpriteRenderer;

        #endregion



        #region Properties

        protected abstract SpriteRenderer Renderer { get; }

        protected abstract Material OutlineMaterial { get; }

        protected abstract float OutlineWidth { get; }

        protected abstract Color OutlineColor { get; }

        protected abstract FactorAnimation OutlineThresholdAnimation { get; }

        #endregion




        #region Abstract implementation

        public override void Enable()
        {
            block = new MaterialPropertyBlock();
            dblock = new MaterialPropertyBlock();
        }


        public override void Disable()
        {
            FinishHighlighting();
        }

        #endregion



        #region Methods

        protected void StartHighlighting()
        {
            if (outlineSpriteRenderer != null)
            {
                CustomDebug.Log("Attempt to start highlighting without finish");
                FinishHighlighting();
            }

            GameObject outlineGO = new GameObject("OutlineSpriteRenderer");
            outlineGO.transform.SetParent(sourceLevelObject.transform);
            outlineGO.transform.localPosition = Vector3.zero;
            outlineGO.transform.localRotation = Quaternion.identity;

            outlineSpriteRenderer = outlineGO.AddComponent<SpriteRenderer>();
            outlineSpriteRenderer.sortingLayerID = Renderer.sortingLayerID;
            outlineSpriteRenderer.sortingOrder = Renderer.sortingOrder + 2;
            outlineSpriteRenderer.sprite = Renderer.sprite;

            outlineSpriteRenderer.material = OutlineMaterial;

            Renderer.GetPropertyBlock(dblock);

            block.SetTexture(ShaderProperties._MainTex, dblock.GetTexture(ShaderProperties._MainTex));
            block.SetFloat(ShaderProperties._OutlineWidth, OutlineWidth);
            block.SetColor(ShaderProperties._OutlineColor, OutlineColor);

            OutlineThresholdAnimation.Play((value) =>
            {
                block.SetFloat(ShaderProperties._ThresholdEnd, value);
                outlineSpriteRenderer.SetPropertyBlock(block);
            }, this);

            Renderer.SetPropertyBlock(block);
        }


        protected void FinishHighlighting()
        {
            DOTween.Kill(this);

            if (outlineSpriteRenderer != null)
            {
                Content.Management.DestroyObject(outlineSpriteRenderer.gameObject);
                outlineSpriteRenderer = null;
            }
        }

        #endregion
    }
}
