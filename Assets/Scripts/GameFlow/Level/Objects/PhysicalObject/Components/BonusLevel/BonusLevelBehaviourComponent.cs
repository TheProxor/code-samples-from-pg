using System;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class BonusLevelBehaviourComponent : PhysicalLevelObjectComponent
    {
        #region Helpers
        
        public enum BonusLevelObjectState
        {
            None            =    0,
            Inactive        =    1,
            Thrown          =    2,
            Deceleration    =    3,
            Stopped         =    4,
            Unstopped       =    5
        }

        #endregion



        #region Fields

        public static event Action<PhysicalLevelObject, BonusLevelObjectState>  OnStateChanged;

        private BonusLevelController bonusLevelController;
        
        private float deceleration;
        private BonusLevelObjectData bonusLevelData;
        private BonusLevelSettings levelSettings;

        private BonusLevelObjectState objectState;

        private bool isComponentEnabled;

        private Vector2 customVelocity;
        private float customAngularVelocity;
        private float initialGravityScale;

        private int defaultLayer;

        #endregion



        #region Public methods

        public override void Initialize(CollisionNotifier notifier, 
                                        Rigidbody2D rigidbody, 
                                        PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);

            ChangeState(BonusLevelObjectState.None);
            isComponentEnabled = GameServices.Instance.LevelEnvironment.Context.IsBonusLevel;

            if (!isComponentEnabled)
            {
                return;
            }
            
            levelSettings = IngameData.Settings.bonusLevelSettings;

            bonusLevelController = GameServices.Instance.LevelControllerService.BonusLevelController;
            bonusLevelData = sourceObject.CurrentData.bonusData;

            Vector2 velocityBeforeDeceleration = bonusLevelData.linearVelocity;
            velocityBeforeDeceleration.y += bonusLevelData.acceleration * levelSettings.decelerationDelay;
            
            deceleration = PhysicsCalculation.CalculateAcceleration(
                velocityBeforeDeceleration.y, 
                0f,
                levelSettings.decelerationDuration);
            
            initialGravityScale = IngameData.Settings.physicalObject.gravityScale;

            rigidbody2D.gravityScale = 0f;
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

            ChangeState(BonusLevelObjectState.Inactive);
        }

        #endregion
        
        
        
        #region Abstract implementation

        public override void Enable()
        {
            if (isComponentEnabled)
            {
                ChangePosition();
                
                CommonUtility.SetObjectActive(sourceLevelObject.gameObject, false);
                
                MonoBehaviourLifecycle.OnFixedUpdate += MonoBehaviourLifecycle_OnFixedUpdate;
                bonusLevelController.OnStageBegun += BonusLevelController_OnStageBegun;
                bonusLevelController.OnDecelerationBegin += BonusLevelController_OnDecelerationBegin;
                bonusLevelController.OnStopObjects += BonusLevelController_OnStopObjects;
                bonusLevelController.OnUnstopObjects += BonusLevelController_OnUnstopObjects;

                defaultLayer = sourceLevelObject.gameObject.layer; 
                sourceLevelObject.gameObject.layer = LayerMask.NameToLayer(PhysicsLayers.BonusLevel);

                sourceLevelObject.MainCollider2D.enabled = true;
            }
        }

        


        public override void Disable()
        {
            if (isComponentEnabled)
            {
                rigidbody2D.gravityScale = initialGravityScale;

                MonoBehaviourLifecycle.OnFixedUpdate -= MonoBehaviourLifecycle_OnFixedUpdate;
                bonusLevelController.OnStageBegun -= BonusLevelController_OnStageBegun;
                bonusLevelController.OnDecelerationBegin -= BonusLevelController_OnDecelerationBegin;
                bonusLevelController.OnStopObjects -= BonusLevelController_OnStopObjects;
                bonusLevelController.OnUnstopObjects -= BonusLevelController_OnUnstopObjects;

                sourceLevelObject.gameObject.layer = defaultLayer;

                sourceLevelObject.MainCollider2D.enabled = true;
            }
        }

        #endregion



        #region Private methods

        private void ThrowObject()
        {
            ChangeState(BonusLevelObjectState.Thrown);
            CommonUtility.SetObjectActive(sourceLevelObject.gameObject, true);
            
            customVelocity = bonusLevelData.linearVelocity;
            customAngularVelocity = bonusLevelData.angularVelocity;
            rigidbody2D.gravityScale = 0f;
            sourceLevelObject.MainCollider2D.enabled = true;
        }


        private void DecelerateObject() =>
            ChangeState(BonusLevelObjectState.Deceleration);
        

        private void StopObject()
        {
            ChangeState(BonusLevelObjectState.Stopped);
            
            customVelocity = Vector2.zero;
        }


        private void UnstopObject()
        {
            ChangeState(BonusLevelObjectState.Unstopped);

            rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            rigidbody2D.gravityScale = initialGravityScale;

            sourceLevelObject.MainCollider2D.enabled = false;
        }


        private void ChangePosition()
        {
            Vector3 prevPosition = sourceLevelObject.transform.position;

            float y = IngameData.Settings.bonusLevelSettings.spawnYPosition;

            prevPosition.y = y;

            sourceLevelObject.transform.position = prevPosition;

            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        }


        private bool IsMatchObject(int stageIndex) =>
            bonusLevelData.stageIndex == stageIndex;


        private void ChangeState(BonusLevelObjectState targetState)
        {
            objectState = targetState;
            OnStateChanged?.Invoke(sourceLevelObject, objectState);
        }

        #endregion



        #region Events handlers
        
        private void MonoBehaviourLifecycle_OnFixedUpdate(float deltaTime)
        {
            if (objectState == BonusLevelObjectState.Deceleration ||
                objectState == BonusLevelObjectState.Thrown)
            {
                float acceleration = objectState == BonusLevelObjectState.Deceleration
                    ? deceleration
                    : bonusLevelData.acceleration;
                
                Vector2 velocity = new Vector2
                {
                    x = customVelocity.x,
                    y = customVelocity.y + acceleration * deltaTime * bonusLevelController.CustomTimeScale
                };

                customVelocity = velocity;
            }
            
            if (objectState == BonusLevelObjectState.Inactive ||
                objectState == BonusLevelObjectState.None ||
                objectState == BonusLevelObjectState.Unstopped)
            {
                return;
            } 
            
            rigidbody2D.velocity = customVelocity * bonusLevelController.CustomTimeScale;
            rigidbody2D.angularVelocity = customAngularVelocity * bonusLevelController.CustomTimeScale;
        }
        
        
        private void BonusLevelController_OnStageBegun(int stageIndex)
        {
            if (IsMatchObject(stageIndex))
            {
                ThrowObject();
            }
        }
        
        private void BonusLevelController_OnDecelerationBegin(int stageIndex)
        {
            if (IsMatchObject(stageIndex))
            {
                DecelerateObject();
            }
        }
        
        
        private void BonusLevelController_OnStopObjects(int stageIndex)
        {
            if (IsMatchObject(stageIndex))
            {
                StopObject();
            }
        }
        
        
        private void BonusLevelController_OnUnstopObjects(int stageIndex)
        {
            if (IsMatchObject(stageIndex))
            {
                UnstopObject();
            }
        }

        #endregion
    }
}

