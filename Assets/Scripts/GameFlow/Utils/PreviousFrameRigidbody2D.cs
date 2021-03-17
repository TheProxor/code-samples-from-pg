using UnityEngine;


namespace Drawmasters
{
    public class PreviousFrameRigidbody2D : IInitializable, IDeinitializable
    {
        #region Fields

        private readonly Rigidbody2D mainRigidbody2D;

        private Vector2 savedVelocity;
        private Vector3 savedPosition;

        #endregion



        #region Properties

        public Vector3 Position { get; private set; }


        public Vector2 Velocity { get; private set; }


        public float Mass => mainRigidbody2D.mass;

        #endregion


        #region IInitializable

        public void Initialize()
        {
            MonoBehaviourLifecycle.OnFixedUpdate += MonoBehaviourLifecycle_OnFixedUpdate;

            Velocity = mainRigidbody2D.velocity;
            savedVelocity = mainRigidbody2D.velocity;

            Position = mainRigidbody2D.position;
            savedPosition = mainRigidbody2D.position;
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            MonoBehaviourLifecycle.OnFixedUpdate -= MonoBehaviourLifecycle_OnFixedUpdate;
        }

        #endregion



        #region Class lifecycle

        public PreviousFrameRigidbody2D(Rigidbody2D rigidbody2D)
        {
            mainRigidbody2D = rigidbody2D;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnFixedUpdate(float deltaTime)
        {
            Velocity = savedVelocity;
            savedVelocity = mainRigidbody2D.velocity;

            Position = savedPosition;
            savedPosition = mainRigidbody2D.position;
        }

        #endregion
    }
}
