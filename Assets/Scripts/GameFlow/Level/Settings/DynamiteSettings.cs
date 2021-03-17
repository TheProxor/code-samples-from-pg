using UnityEngine;
using System;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "DynamiteSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "DynamiteSettings")]
    public class DynamiteSettings : ScriptableObject
    {
        #region Helper types

        [Serializable]
        public class ExplosionData
        {
            public float radius = default;
            public float explosionForce = default;
            public float enemyExplosionForce = default;
            public AnimationCurve distanceDependence = default;
            public float damage = default;
            public float enemyDamage = default;
        }


        [Serializable]
        public abstract class Data
        {
            public ExplosionData explosionData = default;                   
        }


        [Serializable]
        public class PhysicalObjectData : Data
        {
            public PhysicalLevelObjectData data = default;
        }

        [Serializable]
        public class ProjectilesData : Data
        {
            public ProjectileType projectileType = default;
        }

        #endregion



        #region Constants

        public static readonly CollidableObjectType[] ExplosibleTypes = { CollidableObjectType.EnemyTrigger,
                                                                          CollidableObjectType.EnemyStand,
                                                                          CollidableObjectType.PhysicalObject,
                                                                          CollidableObjectType.Spikes };

        #endregion



        #region Fields

        public SpriteRenderer constructorVisualRenderer = default;

        [SerializeField] private List<PhysicalObjectData> explosionDataList = default;
        [SerializeField] private List<ProjectilesData> explosionProjectileDataList = default;

        [NonSerialized] static LayerMask? explosionMask;
        [NonSerialized] static LayerMask? enemiesExplosionMask;

        #endregion



        #region Properties

        public static LayerMask ExplosionMask
        {
            get
            {
                if (explosionMask == null)
                {
                    explosionMask = 1 << LayerMask.NameToLayer(PhysicsLayers.LevelObject);
                    explosionMask |= 1 << LayerMask.NameToLayer(PhysicsLayers.LevelObjectPhysics);
                    explosionMask |= 1 << LayerMask.NameToLayer(PhysicsLayers.Enemy);
                    explosionMask |= 1 << LayerMask.NameToLayer(PhysicsLayers.Acid);
                    explosionMask |= 1 << LayerMask.NameToLayer(PhysicsLayers.Shooter);
                }

                return explosionMask.Value;
            }
        }


        #endregion



        #region Methods

        public bool IsExplosionDataExists(PhysicalLevelObjectData findData)
        {
            bool result = default;

            Data dataContainer = explosionDataList.Find(d => d.data.Equals(findData));

            if (dataContainer != null)
            {
                result = (dataContainer.explosionData != null);
            }

            return result;
        }


        public bool TryFindExplosionData(PhysicalLevelObjectData findData, out ExplosionData explosionData)
        {
            bool result = default;
            explosionData = default;

            Data dataContainer = explosionDataList.Find(d => d.data.Equals(findData));

            if (dataContainer != null)
            {
                explosionData = dataContainer.explosionData;

                result = (explosionData != null);
            }

            if (!result)
            {
                CustomDebug.Log($"Not found explosion data in {this} level object for {findData}");
            }

            return result;
        }


        public bool TryFindExplosionData(ProjectileType projectileType, out ExplosionData explosionData)
        {
            bool result = default;
            explosionData = default;

            Data dataContainer = explosionProjectileDataList.Find(d => d.projectileType == projectileType);

            if (dataContainer != null)
            {
                explosionData = dataContainer.explosionData;

                result = (explosionData != null);
            }

            if (!result)
            {
                CustomDebug.Log($"Not found explosion data in {this} level object for {projectileType}");
            }

            return result;
        }

        #endregion
    }
}
