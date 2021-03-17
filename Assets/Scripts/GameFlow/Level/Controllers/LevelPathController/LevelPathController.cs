using System;
using Drawmasters.AbTesting;
using Drawmasters.Interfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.Ua;
using TMPro;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class LevelPathController : ILevelController
    {
        #region Fields

        public event Action<float> OnPathChanged; // value 0->1 

        private readonly LevelTargetController targetController;
        
        private float maxPathDistance;
        private float currentPathDistance;

        public static IUaAbTestMechanic uaAbTestMechanic = 
            new CommonMechanicAvailability(PrefsKeys.AbTest.UaInfinityDrawingAvailable);
        
        #endregion


        #region Properties

        public bool IsPathDistanceLimit => currentPathDistance >= maxPathDistance;

        public bool IsControllerEnabled
            => uaAbTestMechanic.WasAvailabilityChanged ? 
               uaAbTestMechanic.IsMechanicAvailable : 
               !GameServices.Instance.AbTestService.CommonData.isInfinityDrawingEnabled;
        
        #endregion
        
        
        
        #region Ctor

        public LevelPathController(LevelTargetController _targerController)
        {
            targetController = _targerController;
        }
        
        #endregion



        #region Public methods

        public bool CanShooterDraw(Shooter shooter)
        {
            float pathDistanceOtherShooters = 0f;
            
            foreach (var i in targetController.GetShooters())
            {
                if (i != shooter)
                {
                    pathDistanceOtherShooters += i.DrawingPathDistance;
                }
            }

            return pathDistanceOtherShooters < maxPathDistance;
        }

        #endregion
        
        
        
        #region ILevelController
        public void Initialize()
        {
            if (IsControllerEnabled)
            {
                ShootersInputLevelController.OnDraw += ShootersInputLevelController_OnDraw;

                maxPathDistance = GameServices.Instance.LevelEnvironment.Context.PathDistance;
                maxPathDistance = Mathf.Max(maxPathDistance, float.Epsilon);
            }
        }

        

        public void Deinitialize()
        {
            if (IsControllerEnabled)
            {
                ShootersInputLevelController.OnDraw -= ShootersInputLevelController_OnDraw;
            }
        }
        
        #endregion



        #region Events handlers

        private void ShootersInputLevelController_OnDraw(Shooter shooter, Vector2 touchPosition)
        {
            currentPathDistance = 0f;

            foreach (var i in targetController.GetShooters())
            {
                currentPathDistance += i.DrawingPathDistance;
            }

            float factor = currentPathDistance / maxPathDistance;
            factor = Mathf.Clamp01(factor);
            
            OnPathChanged?.Invoke(factor);
        }

        #endregion
    }
}

