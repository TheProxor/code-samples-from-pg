using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using System;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Drawmasters.Levels
{
    public class PortalObject : MonoBehaviour, IDeinitializable
    {
        #region Helpers

        public enum Type
        {
            None = 0,
            First = 1,
            Second = 2
        }

        #endregion



        #region Fields

        public event Action<PortalObject, ITeleportable> OnPortalEnter;
        public event Action<PortalObject, ITeleportable> OnPortalExit;

        [SerializeField] private CollisionNotifier collisionNotifier = default;
        [SerializeField] private float portalFxWidth = default;

        private EffectHandler idleEffecthandler;

        #endregion



        #region Properties

        public Type CurrentType { get; private set; }

        public float Width => portalFxWidth * transform.localScale.x;

        public Vector3 LeftPoint => transform.position + Vector3.right * Width * 0.5f;

        public Vector3 RightPoint => transform.position - Vector3.right * Width * 0.5f;

        #endregion



        #region Methods

        public void Initialize(Type type)
        {
            CurrentType = type;

            collisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            collisionNotifier.OnCustomTriggerExit2D += CollisionNotifier_OnCustomTriggerExit2D;

            collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
            collisionNotifier.OnCustomCollisionExit2D += CollisionNotifier_OnCustomCollisionExit2D;
        }


        public void Deinitialize()
        {
            collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            collisionNotifier.OnCustomTriggerExit2D -= CollisionNotifier_OnCustomTriggerExit2D;

            collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;
            collisionNotifier.OnCustomCollisionExit2D -= CollisionNotifier_OnCustomCollisionExit2D;

            CurrentType = Type.None;
        }


        public void Show()
        {            
            WeaponSkinType weaponSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(WeaponType.HitmasteresPortalgun);

            string idleEffectKey = CurrentType == Type.First ? EffectKeys.FxWeaponPortalGreen : EffectKeys.FxWeaponPortalOrange;

            idleEffecthandler = EffectManager.Instance.CreateSystem(idleEffectKey, true, parent: transform, transformMode: TransformMode.Local, shouldOverrideLoops: false);

            if (idleEffecthandler != null)
            {
                idleEffecthandler.transform.localScale = Vector3.one;
                idleEffecthandler.Play();
            }
        }


        public void Hide(Action onHided)
        {
            if (idleEffecthandler != null && !idleEffecthandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(idleEffecthandler);
            }

            idleEffecthandler = null;

            onHided?.Invoke();
        }


        public void PlayTeleportedEffect()
        {
            WeaponSkinType weaponSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(WeaponType.HitmasteresPortalgun);

            string enterEffectKey = CurrentType == Type.First ? 
                EffectKeys.FxWeaponPortalGreenInception : 
                EffectKeys.FxWeaponPortalOrangeInception;

            EffectManager.Instance.PlaySystemOnce(enterEffectKey, transform.position, transform.rotation);
        }


        private void ProcessEnter(Collider2D otherCollider)
        {
            if (TryGetTeleportableObject(otherCollider, out ITeleportable teleportable))
            {
                OnPortalEnter?.Invoke(this, teleportable);
            }
        }


        private void ProcessExit(Collider2D otherCollider)
        {
            if (TryGetTeleportableObject(otherCollider, out ITeleportable teleportable))
            {
                OnPortalExit?.Invoke(this, teleportable);
            }
        }


        private bool TryGetTeleportableObject(Collider2D otherCollider, out ITeleportable teleportableResult)
        {
            bool result = false;
            teleportableResult = null;

            CollidableObject collidable = otherCollider.GetComponent<CollidableObject>();

            if (collidable != null)
            {
                Object levelObjectToTeleport = default;

                if (collidable.PhysicalLevelObject != null)
                {
                    levelObjectToTeleport = collidable.PhysicalLevelObject;
                }

                if (collidable.LevelTarget != null)
                {
                    levelObjectToTeleport = collidable.LevelTarget;

                    LevelTargetLimbPart collidedLimbPart = collidable.GetComponent<LevelTargetLimbPart>();
                    LevelTargetLimb collidedLimb = collidable.LevelTarget.Limbs.Find(e => e.LimbParts.Contains(collidedLimbPart));

                    if (collidedLimb != null && collidable.LevelTarget.IsChoppedOffLimb(collidedLimb.name))
                    {
                        levelObjectToTeleport = collidedLimb;
                    }
                }


                if (levelObjectToTeleport is ITeleportable teleportable)
                {
                    teleportableResult = teleportable;
                    result = true;
                }
            }

            return result;
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject go, Collider2D otherCollider) =>
            ProcessEnter(otherCollider);

        private void CollisionNotifier_OnCustomTriggerExit2D(GameObject go, Collider2D otherCollider) =>
            ProcessExit(otherCollider);

        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject go, Collision2D collision2D) =>
            ProcessEnter(collision2D.collider);

        private void CollisionNotifier_OnCustomCollisionExit2D(GameObject go, Collision2D collision2D) =>
            ProcessExit(collision2D.collider);

        #endregion
    }
}
