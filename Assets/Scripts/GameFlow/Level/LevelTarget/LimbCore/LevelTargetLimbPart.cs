using Spine.Unity;
using System;
using System.Reflection;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LevelTargetLimbPart : MonoBehaviour
    {
        #region Fields

        public event Action<CollidableObject, LevelTargetLimbPart> OnCollidableObjectHitted;

        [SerializeField] private CollisionNotifier collisionNotifier = default;

        [SerializeField][SpineBone(fallbackSearchInParents = true)]
        private string boneName = default;

        [SerializeField] private Collider2D mainCollider = default;

        #endregion



        #region Properties

        public Collider2D MainCollider => mainCollider;

        public string BoneName => boneName;

        #endregion



        #region IInitializable

        public void Initialize()
        {
            if (mainCollider != null)
            {
                mainCollider.enabled = true;
            }
        }

        #endregion



        #region Methods


        private void OnEnable()
        {
            collisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
        }


        private void OnDisable()
        {
            collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D collision2D)
        {
            if (collision2D.TryGetComponent(out CollidableObject collidableObject))
            {
                OnCollidableObjectHitted?.Invoke(collidableObject, this);
            }
        }

        #endregion



        #region Editor methods

        #if UNITY_EDITOR

        private void Reset()
        {
            AssignBone();
            AddCollisionNotifier();
            AssignCollider();
        }


        private void OnValidate()
        {
            AssignCollider();
        }


        public void AssignCollider()
        {
            mainCollider = gameObject.GetComponent<Collider2D>();
        }

        /// <summary>
        /// Set appropriate bone if Bounding box was create from inspector
        /// </summary>
        [Sirenix.OdinInspector.Button]
        private void AssignBone()
        {
            boneName = gameObject.name.Replace("[BoundingBox]box_", string.Empty);
        }


        [Sirenix.OdinInspector.Button]
        private void AddCollisionNotifier()
        {
            if (collisionNotifier != null)
            {
                return;
            }

            collisionNotifier = GetComponent<CollisionNotifier>();

            if (collisionNotifier == null)
            {
                collisionNotifier = gameObject.AddComponent<CollisionNotifier>();
            }
            
            FieldInfo field = typeof(CollisionNotifier).GetField("gameObjectReference", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(collisionNotifier, gameObject);
            }
        }

        #endif

        #endregion
    }
}
