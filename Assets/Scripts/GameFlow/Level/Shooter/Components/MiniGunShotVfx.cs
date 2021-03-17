using UnityEngine;
using Drawmasters.Vibration;
using System.Collections.Generic;
using Drawmasters.Pool;
using Drawmasters.Effects;

namespace Drawmasters.Levels
{
    public class MiniGunShotVfx : ShotVfx
    {
        #region Fields

        private readonly List<EffectHandler> handlers = new List<EffectHandler>();

        #endregion



        #region Overrided methods

        public override void Initialize(Shooter _shooter)
        {
            base.Initialize(_shooter);

            handlers.Clear();
        }

        public override void StartGame()
        {
            base.StartGame();

            shooter.OnLookSideChanged += Shooter_OnLookSideChanged;
        }


        public override void Deinitialize()
        {
            shooter.OnLookSideChanged -= Shooter_OnLookSideChanged;

            base.Deinitialize();
        }


        protected override EffectHandler PlayFx(Vector2 direction)
        {
            EffectHandler handler = base.PlayFx(direction);

            string nextShotSfx = SoundGroupKeys.RandomMinigunShotKey;
            ChangeSfx(nextShotSfx);

            VibrationManager.Play(IngameVibrationType.MinigunShot);

            if (handler != null)
            {
                handlers.Add(handler);
            }

            return handler;
        }

        #endregion



        #region Private methods

        private void RepositionVfx(ShooterLookSide side)
        {
            shooter.ProjectileEffectBoneFollowewr.LateUpdate();

            Vector3 worldRotation = shooter.ProjectileEffectSpawnTransform.rotation.eulerAngles;            
                        
            if (side == ShooterLookSide.Left)
            {
                worldRotation.z += 180.0f;
            }
            
            worldRotation.y = 0f;

            foreach(var i in handlers)
            {
                ComponentPool pool = PoolManager.Instance.GetComponentPool(i, false);
                
                if (pool.InPool(i))
                {
                    continue;
                }

                i.transform.position = EffectPosition;
                i.transform.rotation = Quaternion.Euler(worldRotation);
            }
        }

        #endregion



        #region Events handlers

        private void Shooter_OnLookSideChanged()
        {
            RepositionVfx(shooter.LookingSide);
            /*
            MonoBehaviourLifecycle.PlayCoroutine(Reposition());

            IEnumerator Reposition()
            {
                yield return new WaitForEndOfFrame();

                RepositionVfx(shooter.LookingSide);

                yield break;
            }
            */
        }

        #endregion
    }
}
