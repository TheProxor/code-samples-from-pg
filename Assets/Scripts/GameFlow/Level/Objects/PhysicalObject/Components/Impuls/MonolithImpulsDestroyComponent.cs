using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class MonolithImpulsDestroyComponent : ImpulseDestroyComponent
    {
        #region Methods

        private void HandleMonolithCollision()
        {
            float impulsPower = PhysicsCalculation.GetImpulsMagnitudeWithMonolith(sourceLevelObject.PreviousFrameRigidbody2D);

            DestroyObject(sourceLevelObject, impulsPower);

            bool isLogging = (impulsPower / sourceLevelObject.Strength) >= minStrengthPercentDamageToLog;

            if (isLogging)
            {
                bool isDestroyed = (impulsPower > sourceLevelObject.Strength);

                string prefix = (isDestroyed) ? ("Destroyed") : ("Damaged");

                string logText = $"{prefix} {sourceLevelObject.name} with impuls {impulsPower}.";

                logText += $" Velocity was {Math.Round(sourceLevelObject.PreviousFrameRigidbody2D.Velocity.magnitude, 1)}";
                
                InvokeLogEvent(logText);
            }
        }

        #endregion



        #region Events handlers

        protected override void HandleCollision(GameObject go, GameObject otherGameObject)
        {
            CollidableObject collidableObject = otherGameObject.GetComponent<CollidableObject>();

            if (collidableObject != null &&
                collidableObject.Type == CollidableObjectType.Monolith)
            {
                HandleMonolithCollision();
            }
        }

        #endregion
    }
}
