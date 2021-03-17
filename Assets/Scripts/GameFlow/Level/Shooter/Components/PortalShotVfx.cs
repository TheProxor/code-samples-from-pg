using Drawmasters.Effects;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class PortalShotVfx : ShotVfx
    {
        #region Fields

        private bool allowPlayFx;

        #endregion



        #region Methods

        public override void Initialize(Shooter _shooter)
        {
            base.Initialize(_shooter);
                                   
            ChangeShotVfx(PortalObject.Type.First);

            PortalController.OnTypeChanged += ChangeShotVfx;

            allowPlayFx = true;
        }


        public override void Deinitialize()
        {
            PortalController.OnTypeChanged -= ChangeShotVfx;

            base.Deinitialize();
        }


        protected override EffectHandler PlayFx(Vector2 direction)
        {
            if (!allowPlayFx)
            {
                return null;
            }

            return base.PlayFx(direction);
        }

        #endregion



        #region Events handlers

        private void ChangeShotVfx(PortalObject.Type type)
        {
            shotEffectName = type == PortalObject.Type.First ? EffectKeys.FxWeaponPortalGunShotGreen : EffectKeys.FxWeaponPortalGunShotOrange;

            string  shotSoundFxKey = type == PortalObject.Type.First ? 
                AudioKeys.Ingame.WEAPON_PORTALGUN_01 : 
                AudioKeys.Ingame.WEAPON_PORTALGUN_02;

            ChangeSfx(shotSoundFxKey);
        }

        #endregion
    }
}

