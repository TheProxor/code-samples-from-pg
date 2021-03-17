using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class EmptyRopeComponent : RopeComponent
    {
        #region Fields

        private readonly float emptySegmentsScale;

        private List<RopeSegment> ropeSegments;

        #endregion



        #region Class lifecycle

        public EmptyRopeComponent(float _emptySegmentsScale)
        {
            emptySegmentsScale = _emptySegmentsScale;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            RopeCreateComponent.OnRopeGenerated += RopeCreateComponent_OnRopeGenerated;
            RopeCreateComponent.OnRopeBecomeEmpty += RopeCreateComponent_OnRopeBecomeEmpty;
        }


        public override void Disable()
        {
            RopeCreateComponent.OnRopeGenerated -= RopeCreateComponent_OnRopeGenerated;
            RopeCreateComponent.OnRopeBecomeEmpty -= RopeCreateComponent_OnRopeBecomeEmpty;
        }


        private void SetupSegmentsGravityScale(float value)
        {
            foreach (var segment in ropeSegments)
            {
                segment.MainRigidbody2D.gravityScale = value;
            }
        }

        #endregion



        #region Events handlers

        private void RopeCreateComponent_OnRopeGenerated(Rope anotherRope, List<RopeSegment> _ropeSegments)
        {
            if (rope == anotherRope)
            {
                ropeSegments = _ropeSegments;
                SetupSegmentsGravityScale(rope.DefaultSegmentsScale);
            }
        }


        private void RopeCreateComponent_OnRopeBecomeEmpty(Rope anotherRope)
        {
            if (rope == anotherRope)
            {
                SetupSegmentsGravityScale(emptySegmentsScale);
            }
        }

        #endregion
    }
}
