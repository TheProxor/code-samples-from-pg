using UnityEngine;


namespace Drawmasters
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Rigidbody2DInfo : MonoBehaviour
    {
        #region Fields

        [SerializeField] private bool needEveryFrameTracking = default;

        private Rigidbody2D body;

        private int trackFrame = 0;

        #endregion



        #region Properties

        private Rigidbody2D Body
        {
            get
            {
                if (body == null)
                {
                    body = gameObject.GetComponent<Rigidbody2D>();
                }

                return body;
            }
        }

        #endregion



        #region Unity lifecycle

        private void FixedUpdate()
        {
            if (needEveryFrameTracking)
            {
                string message = $"{gameObject.name} velocity on {trackFrame} frame: {Body.velocity}";

                print(message);
            }
            else
            {
                trackFrame = 0;
            }
        }

        #endregion



        #region Editor

        [Sirenix.OdinInspector.Button]
        private void PrintVelocity()
        {
            string message = $"{gameObject.name} velocity: {Body.velocity}";

            print(message);
        }

        #endregion
    }
}

