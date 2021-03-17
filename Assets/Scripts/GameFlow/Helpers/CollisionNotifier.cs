using System;
using UnityEngine;


namespace Drawmasters
{
    public class CollisionNotifier : MonoBehaviour
    {
        #region Fields

        public event Action<GameObject, Collider2D> OnCustomTriggerEnter2D;
        public event Action<GameObject, Collider2D> OnCustomTriggerExit2D;

        public event Action<GameObject, Collision2D> OnCustomCollisionEnter2D;
        public event Action<GameObject, Collision2D> OnCustomCollisionExit2D;

        [SerializeField] private GameObject gameObjectReference = default;

        #endregion



        #region Unity lifecycle

        private void OnTriggerEnter2D(Collider2D collisionCollider)
        {
            OnCustomTriggerEnter2D?.Invoke(gameObjectReference, collisionCollider);
        }


        private void OnTriggerExit2D(Collider2D collisionCollider)
        {
            OnCustomTriggerExit2D?.Invoke(gameObjectReference, collisionCollider);
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            OnCustomCollisionEnter2D?.Invoke(gameObjectReference, collision);
        }


        private void OnCollisionExit2D(Collision2D collision)
        {
            OnCustomCollisionExit2D?.Invoke(gameObjectReference, collision);
        }

        #endregion
    }
}
