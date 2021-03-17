using Drawmasters.Effects;
using System;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class GravitygunAimVfx : MonoBehaviour
    {
        #region Fields

        private const float fxScalePerUnit = 0.02f;
        private const float fxShapeScalePerUnit = 0.00024589880f;

        private int[] effectIndexesToScale;
        private int[] effectIndexesToScaleShape;

        private EffectHandler effectHandler;

        #endregion



        #region Methods

        public void Initialize(WeaponSkinType weaponSkinType)
        {   
            effectHandler = EffectManager.Instance.CreateSystem(EffectKeys.FxWeaponGraviGunAiming,
                                                                true,
                                                                Vector3.zero,
                                                                Quaternion.identity,
                                                                transform,
                                                                TransformMode.World,
                                                                false);


            effectIndexesToScale = new int[] { 1, 3, 5 };
            effectIndexesToScaleShape = new int[] { 4 };
        }


        public void Deinitialize()
        {
            if (effectHandler != null && !effectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(effectHandler);
            }

            effectHandler = null;
        }


        public void Play() => effectHandler.Play();


        public void StopDrawing()
        {
            effectHandler.Pause();
            effectHandler.Clear();
        }


        public void RecalculateDistance(float rayDistance)
        {
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
