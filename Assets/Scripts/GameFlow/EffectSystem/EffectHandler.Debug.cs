using UnityEngine;


namespace Drawmasters.Effects
{
    public partial class EffectHandler
    {
        #region Editor Methods

        [Sirenix.OdinInspector.Button("Fill particle systems and trails")]
        private void FillAllFields()
        {
            ParticleSystem[] particleSystemsInChildrens = GetComponentsInChildren<ParticleSystem>();
            TrailRenderer[] trailsRenderersInChildrens = GetComponentsInChildren<TrailRenderer>();

            particleSystems.Clear();
            trailsRenderers.Clear();

            particleSystems.AddRange(particleSystemsInChildrens);
            trailsRenderers.AddRange(trailsRenderersInChildrens);
        }



        private void Reset()
        {
            FillAllFields();
        }

        #endregion
    }
}
