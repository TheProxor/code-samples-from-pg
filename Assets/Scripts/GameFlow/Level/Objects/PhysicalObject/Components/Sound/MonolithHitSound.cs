using Modules.Sound;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class MonolithHitSound : PhysicalLevelObjectComponent
    {
        #region Fields

        private static readonly string[] HitSoundKeys = { AudioKeys.Ingame.SPHEREWOODHIT01,
                                                          AudioKeys.Ingame.SPHEREWOODHIT02,
                                                          AudioKeys.Ingame.SPHEREWOODHIT03,
                                                          AudioKeys.Ingame.SPHEREWOODHIT04
                                                        };
        #endregion



        #region Overrided methods

        public override void Enable()
        {
            collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
        }

        

        public override void Disable()
        {
            collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject go, Collision2D collision)
        {
            CollidableObject collidable = go.GetComponent<CollidableObject>();

            if (collidable != null &&
                collidable.PhysicalLevelObject != null)
            {
                float velocity = collidable.PhysicalLevelObject.PreviousFrameRigidbody2D.Velocity.magnitude;

                bool canPLay = (velocity >= IngameData.Settings.physicalObject.velocityForHitSound);
                if (canPLay)
                {
                    string hitSound = HitSoundKeys.RandomObject();

                    SoundManager.Instance.PlayOneShot(hitSound);
                }
            }
        }

        #endregion
    }
}
