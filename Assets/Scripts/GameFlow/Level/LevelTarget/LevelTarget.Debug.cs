using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace Drawmasters.Levels
{
    public abstract partial class LevelTarget
    {
        #region Fields

        private const string physicSkeletonParentName = "PhysicSkeleton";

        private const string pattern = "[BoundingBox]box_";

        private string namePattern = "foot_";

        private static Vector2[] polygonPoints = new Vector2[] { new Vector2(-0.8f, 0.3f),
                                                                 new Vector2( 1.5f, 0.3f),
                                                                 new Vector2(1.5f, -0.9f),
                                                                 new Vector2(-0.8f, -0.9f) };

        [SerializeField] private bool isDebugRagdoll = default;

        #endregion



        #region Methods

#if UNITY_EDITOR

        private void OnGUI()
        {
            if (isDebugRagdoll)
            {
                if (GUI.Button(new Rect(100, 100, 200, 200), "Apply Ragdoll"))
                {
                    ApplyRagdoll();
                }
            }
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


        #region Editor
#if UNITY_EDITOR

        [Sirenix.OdinInspector.Button]
        private void AssignValues()
        {
            if (standRigidbody == null)
            {
                standRigidbody = gameObject.GetComponent<Rigidbody2D>();
            }
            
            var searchStandCollider = from collider in gameObject.GetComponentsInChildren<PolygonCollider2D>()
                where !collider.isTrigger select collider;


            standColliders = new List<Collider2D>(searchStandCollider);
    
            foreach (var poly in searchStandCollider)
            {
                AddCollidableObjectComponent(poly.gameObject, CollidableObjectType.EnemyStand);
            }

            var searchTriggerColliders = from collider in gameObject.GetComponentsInChildren<PolygonCollider2D>()
                where collider.isTrigger select collider;

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

            gameObject.SetLayerRecursively(LayerMask.NameToLayer(PhysicsLayers.Enemy));
        }

        [Sirenix.OdinInspector.Button]    
        void CopyColliders()
        {
            GameObject physicSkeleton = new GameObject(physicSkeletonParentName);

            physicSkeleton.transform.parent = gameObject.transform;
            physicSkeleton.transform.localPosition = Vector3.zero;
            physicSkeleton.transform.localScale = Vector3.one;

            Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();

            foreach(var c in colliders)
            {
                if (c is PolygonCollider2D)
                {
                    if (!c.isTrigger)
                    {
                        continue;
                    }

                    string oldName = c.name;
                    string newName = string.Empty;

                    int patternIndex = oldName.IndexOf(pattern);
                    int substractIndex = patternIndex + pattern.Length;
                    int charsCount = oldName.Length - pattern.Length;

                    newName = oldName.Substring(substractIndex, charsCount);

                    GameObject go = new GameObject(newName);
                    go.transform.parent = physicSkeleton.transform;
                    go.transform.position = c.transform.position;
                    go.transform.rotation = c.transform.rotation;
                    go.transform.localScale = c.transform.localScale;

                    PolygonCollider2D polyCollider = go.AddComponent<PolygonCollider2D>();

                    PolygonCollider2D oldPolyCollider = c as PolygonCollider2D;

                    if (oldPolyCollider == null)
                    {
                        continue;
                    }

                    polyCollider.isTrigger = !oldPolyCollider.isTrigger;
                    polyCollider.points = oldPolyCollider.points;
                    polyCollider.offset = oldPolyCollider.offset;
                }
            }

            VerifyCollider();
        }


        void VerifyCollider()
        {
            int changesCount = 0;
            float minYPosition = float.MaxValue;

            PolygonCollider2D[] polygonColliders = gameObject.GetComponentsInChildren<PolygonCollider2D>();

            for (int i = 0; i < polygonColliders.Length; i++)
            {
                bool isNameMatching = (polygonColliders[i].name.IndexOf(namePattern) != -1);

                if (isNameMatching &&
                    minYPosition > polygonColliders[i].transform.position.y)
                {
                    minYPosition = polygonColliders[i].transform.position.y;
                }
            }

            for (int i = 0; i < polygonColliders.Length; i++)
            {
                if (polygonColliders[i] == null)
                {
                    continue;
                }
    

                bool isNameMatching = (polygonColliders[i].name.IndexOf(namePattern) != -1);

                if (isNameMatching)
                {
                    polygonColliders[i].transform.position = polygonColliders[i].transform.position.SetY(minYPosition);
                    polygonColliders[i].transform.rotation = Quaternion.Euler(Vector3.zero);

                    polygonColliders[i].pathCount = 1;
                    polygonColliders[i].SetPath(0, polygonPoints);
                    changesCount++;
                }
            }
        }

#endif
        #endregion

#endif

        #endregion
    }
}
