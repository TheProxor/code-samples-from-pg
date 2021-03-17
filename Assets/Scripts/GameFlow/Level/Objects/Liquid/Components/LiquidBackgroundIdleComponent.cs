using Drawmasters.Effects;

namespace Drawmasters.Levels
{
    public class LiquidBackgroundIdleComponent : LiquidGraphicVisualComponent
    {
        #region Fields

        private EffectHandler backgroundEffectHandler;
        private EffectHandler backgroundSmokeEffectHandler;

        #endregion

        

        #region Methods

        public override void Enable()
        {
            base.Enable();

            liquid.SpriteShapeRenderer.material.color = visualSettings.textureColor;

            backgroundEffectHandler = EffectManager.Instance.CreateSystem(EffectKeys.FxAcidIdleBubble,
                               true,
                               liquid.transform.position,
                               liquid.transform.rotation,
                               liquid.transform);

            if (backgroundEffectHandler != null)
            {
                foreach (var ps in backgroundEffectHandler.ParticleSystems)
                {
                    var module = ps.shape;
                    module.scale = liquid.LoadedData.size.ToVector3();
                }

                backgroundEffectHandler.Play();
            }


            backgroundSmokeEffectHandler = EffectManager.Instance.CreateSystem(EffectKeys.FxAcidIdleSmoke,
                   true,
                   liquid.transform.position.SetY(liquid.transform.position.y + liquid.LoadedData.size.y * 0.5f),
                   liquid.transform.rotation,
                   liquid.transform);

            if (backgroundSmokeEffectHandler != null)
            {
                foreach (var ps in backgroundSmokeEffectHandler.ParticleSystems)
                {
                    var module = ps.shape;
                    module.radius = liquid.LoadedData.size.x * 0.5f;
                }

                backgroundSmokeEffectHandler.Play();
            }
        }


        public override void Disable()
        {
            if (backgroundEffectHandler != null && !backgroundEffectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(backgroundEffectHandler);
            }
            backgroundEffectHandler = null;


            if (backgroundSmokeEffectHandler != null && !backgroundSmokeEffectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(backgroundSmokeEffectHandler);
            }
            backgroundSmokeEffectHandler = null;
        }

        #endregion
    }
}
