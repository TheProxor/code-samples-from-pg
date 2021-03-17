using Spine.Unity;
using Spine.Unity.Examples;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ShooterSkinLink : MonoBehaviour
    {
        #region Fields

        [SerializeField] private SkeletonAnimation skeletonAnimation = default;
        [SerializeField] private Transform forcemeterFxRoot = default;
        [SerializeField] private Transform forcemeterHammerFxRoot = default;
        [SerializeField] private List<Collider2D> standColliders = default;

        [SerializeField] private SkeletonRagdoll2D ragdoll2D = default;
        [SerializeField] private Renderer currentRenderer = default; 

         [SerializeField] private List<LevelTargetLimbPart> limbsParts = default;
        [SerializeField] private List<LevelTargetLimb> limbs = default;

        [SerializeField] private ShooterAimingAnimation aimingAnimation = default;

        [SerializeField] private SkinSkeletonType skinSkeletonType = default;
        
        [SerializeField] private SkeletonRenderSeparator skeletonRenderSeparator = default;

        #endregion



        #region Properties

        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        public Transform ForcemeterFxRoot => forcemeterFxRoot;

        public Transform ForcemeterHammerFxRoot => forcemeterHammerFxRoot;

        public List<Collider2D> StandColliders => standColliders;

        public SkeletonRagdoll2D Ragdoll2D => ragdoll2D;

        public List<LevelTargetLimbPart> LimbsParts => limbsParts;

        public List<LevelTargetLimb> Limbs => limbs;

        public ShooterAimingAnimation AimingAnimation => aimingAnimation;

        public SkinSkeletonType SkinSkeletonType => skinSkeletonType;

        public Renderer Renderer => currentRenderer;
        
        public SkeletonRenderSeparator SkeletonRenderSeparator => skeletonRenderSeparator;

        #endregion



        #region Editor
#if UNITY_EDITOR

        [Sirenix.OdinInspector.Button]
        private void AssignValues()
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            ragdoll2D = GetComponentInChildren<SkeletonRagdoll2D>();
            aimingAnimation = GetComponentInChildren<ShooterAimingAnimation>();

            var searchStandCollider = from collider in gameObject.GetComponentsInChildren<PolygonCollider2D>()
                                      where !collider.isTrigger
                                      select collider;


            standColliders = new List<Collider2D>(searchStandCollider);

            foreach (var poly in searchStandCollider)
            {
                AddCollidableObjectComponent(poly.gameObject, CollidableObjectType.EnemyStand);
            }

            var searchTriggerColliders = from collider in gameObject.GetComponentsInChildren<PolygonCollider2D>()
                                         where collider.isTrigger
                                         select collider;

            foreach (var poly in searchTriggerColliders)
            {
                AddCollidableObjectComponent(poly.gameObject, CollidableObjectType.EnemyTrigger);
            }

            var searchLimbsParts = from limbPart in gameObject.GetComponentsInChildren<LevelTargetLimbPart>()
                                   select limbPart;

            limbsParts = new List<LevelTargetLimbPart>(searchLimbsParts);
            limbsParts.ForEach(limb => limb.AssignCollider());

            var searchLimbs = from limb in gameObject.GetComponentsInChildren<LevelTargetLimb>()
                              select limb;

            limbs = new List<LevelTargetLimb>(searchLimbs);

            gameObject.SetLayerRecursively(LayerMask.NameToLayer(PhysicsLayers.Shooter));
        }


        private void AddCollidableObjectComponent(GameObject workGameObject, 
                                                  CollidableObjectType type)
        {
            CollidableObject collidable = workGameObject.GetComponent<CollidableObject>();
            if (collidable == null)
            {
                collidable = workGameObject.AddComponent<CollidableObject>();
            }

            if (collidable != null)
            {
                FieldInfo gameObjectField = typeof(CollidableObject).GetField("gameObjectReference", BindingFlags.NonPublic | BindingFlags.Instance);

                if (gameObjectField != null)
                {
                    gameObjectField.SetValue(collidable, gameObject);
                }

                FieldInfo typeField = typeof(CollidableObject).GetField("type", BindingFlags.NonPublic | BindingFlags.Instance);

                if (typeField != null)
                {
                    typeField.SetValue(collidable, type);
                }
            }
        }
#endif

        #endregion
    }
}
