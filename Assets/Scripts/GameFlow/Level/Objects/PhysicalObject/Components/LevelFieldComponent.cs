using UnityEngine;


namespace Drawmasters.Levels
{
    public class LevelFieldComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        private bool isInitialized;

        private Camera gameCamera;

        private Bounds levelFieldBounds;

        #endregion



        #region Methods

        public override void Enable()
        {       
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        public override void Disable()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }


        public override void Initialize(CollisionNotifier notifier, 
                                        Rigidbody2D rigidbody, 
                                        PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);

            gameCamera = Camera.main;

            Bounds sourceBounds = IngameData.Settings.level.levelFieldBounds;

            levelFieldBounds = ClampBounds(gameCamera, sourceBounds);

            isInitialized = true;
        }


        private Bounds ClampBounds(Camera forCamera, Bounds bounds)
        {
            Bounds result = default;

            Vector2 lowerLeftCorner = forCamera.ScreenToWorldPoint(Vector3.zero);
            Vector2 upperRightCorner = forCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));

            Vector2 boundSize = default;

            boundSize.x = Mathf.Max(upperRightCorner.x - lowerLeftCorner.x, bounds.size.x);
            boundSize.y = Mathf.Max(upperRightCorner.y - lowerLeftCorner.y, bounds.size.y);

            result.size = boundSize;
            result.center = forCamera.transform.position;

            return result;
        }

        #endregion


        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float delta)
        {
            if (!isInitialized)
            {
                return;
            }

            bool isObjectOut = CommonUtility.IsOutOfBounds(sourceLevelObject.transform, 
                                                           levelFieldBounds, 
                                                           false);
            if (isObjectOut)
            {
                sourceLevelObject.MarkOutOfZone();
                sourceLevelObject.DestroyObject();
            }
        }

        #endregion
    }

}
