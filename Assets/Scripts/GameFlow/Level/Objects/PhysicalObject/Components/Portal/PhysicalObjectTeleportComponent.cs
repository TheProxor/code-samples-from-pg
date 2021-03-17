using UnityEngine;

namespace Drawmasters.Levels
{
    public class PhysicalObjectTeleportComponent : PhysicalLevelObjectComponent
    {
        #region Methods

        public override void Enable()
        {
            sourceLevelObject.OnShouldTeleport += SourceLevelObject_OnShouldTeleport;
        }


        public override void Disable()
        {
            sourceLevelObject.OnShouldTeleport -= SourceLevelObject_OnShouldTeleport;
        }

        #endregion



        #region Events handlers

        private void SourceLevelObject_OnShouldTeleport(PortalObject enteredPortal, PortalObject exitPortal)
        {
            PortalsSettings portalsSettings = IngameData.Settings.portalsSettings;

            Vector2 previosFrameVelocity = sourceLevelObject.PreviousFrameRigidbody2D.Velocity;

            float velocityMagnitude = previosFrameVelocity.magnitude;
            velocityMagnitude *= portalsSettings.teleportedObjectsVelocityCoefficient;

            float minVelocityMagnitude = portalsSettings.MinVelocityMagnitudeForPortalExit(sourceLevelObject.PhysicalData);
            float maxVelocityMagnitude = portalsSettings.MaxVelocityMagnitudeForPortalExit(sourceLevelObject.PhysicalData);

            velocityMagnitude = Mathf.Clamp(velocityMagnitude, minVelocityMagnitude, maxVelocityMagnitude);

            Vector2 velocityVector = PortalController.CalculateEndDirection(enteredPortal, exitPortal, previosFrameVelocity);
            sourceLevelObject.Rigidbody2D.velocity = velocityVector * velocityMagnitude;

            float portalExitOffset = portalsSettings.FindExitOffset(sourceLevelObject.PhysicalData);
            Vector2 endPosition = PortalController.CalculateEndObjectPosition(enteredPortal, exitPortal, sourceLevelObject.PreviousFrameRigidbody2D.Position, portalExitOffset);
            sourceLevelObject.Rigidbody2D.position = endPosition;
        }

        #endregion
    }
}
