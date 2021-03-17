using System.Collections.Generic;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class RopeRendererComponent : RopeComponent
    {
        #region Fields

        private readonly LineRenderer lineRenderer;

        private CurvedLineRenderer curvedLineRenderer;

        #endregion



        #region Class lifecycle

        public RopeRendererComponent(LineRenderer _lineRenderer) : base()
        {
            lineRenderer = _lineRenderer;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            RopeCreateComponent.OnRopeGenerated += RopeCreateComponent_OnRopeGenerated;
            RopeCreateComponent.OnRopeDestroyed += RopeCreateComponent_OnRopeDestroyed;
        }


        public override void Disable()
        {
            RopeCreateComponent.OnRopeGenerated -= RopeCreateComponent_OnRopeGenerated;
            RopeCreateComponent.OnRopeDestroyed -= RopeCreateComponent_OnRopeDestroyed;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            curvedLineRenderer?.StopRendering();
        }


        private CurvedLineRenderer CreateCurvedLineRenderer(List<RopeSegment> ropeSegments)
        {
            CurvedLineRenderer result = new CurvedLineRenderer(lineRenderer, ropeSegments);
            result.StartRendering();

            return result;
        }

        #endregion



        #region Events handlers

        private void RopeCreateComponent_OnRopeGenerated(Rope anotherRope, List<RopeSegment> ropeSegments)
        {
            if (rope == anotherRope)
            {
                curvedLineRenderer = CreateCurvedLineRenderer(ropeSegments);

                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    foreach (var segment in ropeSegments)
                    {
                        segment.Enable();

                        if (segment != ropeSegments.First())
                        {
                            segment.MarkAnchorAutoConfigurable();
                        }
                    }
                }, CommonUtility.OneFrameDelay);
            }
        }


        private void RopeCreateComponent_OnRopeDestroyed(Rope anotherRope)
        {
            if (rope == anotherRope &&
                curvedLineRenderer != null)
            {
                curvedLineRenderer.StopRendering();
            }
        }

        #endregion
    }
}
