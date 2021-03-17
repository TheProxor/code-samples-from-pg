using System;
using Drawmasters.Effects;
using UnityEngine;


namespace Drawmasters.Utils
{
    public class LineFx
    {
        #region Nested types

        [Serializable]
        public class Settings
        {
            [Enum(typeof(EffectKeys))] public string effectKey = default;

            public float fxScalePerUnit = default;
            public float fxShapeScalePerUnit = default;

            public int[] effectIndexesToScale = default;
            public int[] effectIndexesToScaleShape = default;
        }

        #endregion



        #region Fields

        private float fxScalePerUnit = 0.02f;
        private float fxShapeScalePerUnit = 0.00024589880f;

        private int[] effectIndexesToScale = Array.Empty<int>();
        private int[] effectIndexesToScaleShape = Array.Empty<int>();

        private EffectHandler effectHandler;

        #endregion



        #region Properties

        public Transform EffectHandlerTransform => effectHandler.transform;

        #endregion



        #region Methods

        public void Initialize(string _effectKey, Transform parent = null) =>
            Initialize(_effectKey, Array.Empty<int>(), Array.Empty<int>(), parent);


        public void Initialize(Settings settings, Transform parent = null)
        {
            Initialize(settings.effectKey, settings.effectIndexesToScale, settings.effectIndexesToScaleShape, parent);

            fxScalePerUnit = settings.fxScalePerUnit;
            fxShapeScalePerUnit = settings.fxShapeScalePerUnit;
        }


        public void Initialize(string _effectKey, int[] _effectIndexesToScale, int[] _effectIndexesToScaleShape, Transform parent = null)
        {
            effectHandler = EffectManager.Instance.CreateSystem(_effectKey,
                                                                true,
                                                                Vector3.zero,
                                                                Quaternion.identity,
                                                                parent,
                                                                TransformMode.World,
                                                                false);

            effectIndexesToScale = _effectIndexesToScale;
            effectIndexesToScaleShape = _effectIndexesToScaleShape;
        }

        public void SetFxScalePerUnit(float value) =>
            fxScalePerUnit = value;


        public void Deinitialize()
        {
            if (effectHandler != null && !effectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(effectHandler);
            }

            effectHandler = null;
        }


        public void Play() =>
            effectHandler.Play();


        public void StopDrawing()
        {
            effectHandler.Pause();
            effectHandler.Clear();
        }


        public void RecalculateDistance(float rayDistance)
        {
            if (effectHandler == null)
            {
                return;
            }

            float scale = rayDistance * fxScalePerUnit;
            float shapeScale = rayDistance * fxShapeScalePerUnit;

            for (int i = 0; i < effectHandler.ParticleSystems.Count; i++)
            {
                if (Array.Exists(effectIndexesToScale, e => e == i))
                {
                    Transform effectTransform = effectHandler.ParticleSystems[i].transform;
                    effectTransform.localScale = effectTransform.localScale.SetX(scale);
                }
                else if (Array.Exists(effectIndexesToScaleShape, e => e == i))
                {
                    var module = effectHandler.ParticleSystems[i].shape;
                    module.radius = shapeScale;
                    module.position = module.position.SetX(rayDistance * 0.5f);
                }
            }
        }

        #endregion
    }
}
