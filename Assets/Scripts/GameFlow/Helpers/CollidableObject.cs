using UnityEngine;
using Drawmasters.Levels;


namespace Drawmasters
{
    public class CollidableObject : MonoBehaviour
    {
        #region Fields

        [SerializeField] private GameObject gameObjectReference = default;
        [SerializeField] private CollidableObjectType type = default;

        [HideInInspector][SerializeField]
        private Projectile projectile = default;

        [HideInInspector][SerializeField]
        private LevelObject anyLevelObject = default;

        [HideInInspector]
        [SerializeField]
        private PortalObject portalObject = default;

        #endregion



        #region Properties

        public CollidableObjectType Type => type;


        public bool HasValue => AnyLevelObject != null ||
                                Projectile != null;


        public Projectile Projectile
        {
            get
            {
                if (projectile == null &&
                    gameObjectReference != null)
                {
                    projectile = gameObjectReference.GetComponent<Projectile>();
                }

                return projectile;
            }
        }


        public LevelObject AnyLevelObject
        {
            get
            {
                if (anyLevelObject == null &&
                    gameObjectReference != null)
                {
                    anyLevelObject = gameObjectReference.GetComponent<LevelObject>();
                }

                return anyLevelObject;
            }
        }

        public ICoinCollector CoinCollector
        {
            get
            {
                if (gameObjectReference != null)
                {
                    return gameObjectReference.GetComponent<ICoinCollector>();
                }

                return default;
            }
        }


        public LevelTarget LevelTarget => AnyLevelObject as LevelTarget;

        public PhysicalLevelObject PhysicalLevelObject => AnyLevelObject as PhysicalLevelObject;

        public Shooter Shooter => AnyLevelObject as Shooter;

        public LiquidLevelObject LiquidLevelObject => AnyLevelObject as LiquidLevelObject;

        public LevelObjectMonolith Monolith => AnyLevelObject as LevelObjectMonolith;

        public PortalObject PortalObject
        {
            get
            {
                if (portalObject == null &&
                    gameObjectReference != null)
                {
                    portalObject = gameObjectReference.GetComponent<PortalObject>();
                }

                return portalObject;
            }
        }

        #endregion



        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameObjectReference != null)
            {
                anyLevelObject = gameObjectReference.GetComponent<LevelObject>();
                projectile = gameObjectReference.GetComponent<Projectile>();
                portalObject = gameObjectReference.GetComponent<PortalObject>();
            }
        }
#endif
        #endregion
    }
}
