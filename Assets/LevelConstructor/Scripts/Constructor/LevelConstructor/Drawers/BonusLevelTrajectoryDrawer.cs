using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class BonusLevelTrajectoryDrawer : MonoBehaviour
    {
        #region Fields

        [SerializeField] private LineRenderer solidRenderer = default;
        [SerializeField] private LineRenderer intermittenRenderer = default;

        #endregion
        
        
        
        #region Properties
        
        public EditorLevelObject LevelObject { get; set; }
        
        #endregion


        
        #region Public methods

        public void StartDraw(EditorLevelObject levelObject)
        {
            LevelObject = levelObject;
        }


        public void StopDraw()
        {
            LevelObject = null;
        }
        
        #endregion
        
        
        
        #region Unity lifecycle

        private void Update()
        {
            Draw();
        }

        #endregion
        
        
        
        #region Private methods

        private void Draw()
        {
            if (LevelObject == null)
            {
                return;
            }
            
            BonusLevelSettings levelSettings = IngameData.Settings.bonusLevelSettings;
            
            Vector3 from = LevelObject.transform.position;
            from.y = levelSettings.spawnYPosition;
         
            Vector2 velocity = LevelObject.BonusLevelVelocity;
            float acceleration = LevelObject.BonusLevelAcceleration;

            float t = levelSettings.decelerationDelay;

            float offset = velocity.y * t + acceleration * t * t * 0.5f;

            Vector3 to = new Vector3
            {
                x = from.x,
                y = levelSettings.spawnYPosition + offset
            };

            solidRenderer.positionCount = 2;
            solidRenderer.SetPosition(0, from);
            solidRenderer.SetPosition(1, to);

            float decelerationDuration = levelSettings.decelerationDuration;

            Vector2 endVelocity = velocity;
            endVelocity.y = velocity.y + acceleration * t;
            
            float deceleration = PhysicsCalculation.CalculateAcceleration(
                endVelocity.y, 
                0f,
                decelerationDuration);

            float yy = endVelocity.y * decelerationDuration +
                       deceleration * decelerationDuration * decelerationDuration * 0.5f;
            
            Vector3 toto = new Vector3
            {
                x = to.x,
                y = to.y + yy
            };

            intermittenRenderer.positionCount = 2;
            intermittenRenderer.SetPosition(0, to);
            intermittenRenderer.SetPosition(1, toto);
        }
        
        #endregion
    }
}

