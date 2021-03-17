using UnityEngine;

namespace Drawmasters.Geometry
{
    public class MathSegment : MathRay
    {
        #region Fields

        private Vector2 point0;
        private Vector2 point1;

        #endregion



        #region Lifecycle

        public MathSegment(Vector2 p0, Vector2 p1) :
            base(p0, p1)
        {
            point0 = p0;
            point1 = p1;
        }

        #endregion



        #region Overrided methods

        public override bool IsPointLay(Vector2 p, float precision)
        {
            bool isLayOnRay = base.IsPointLay(p, precision);

            bool isInCommonBounds = (point0.x <= p.x) && (p.x <= point1.x);
            bool isInInvertBounds = (point1.x <= p.x) && (p.x <= point0.x);

            bool isInBounds = isInCommonBounds ^ isInInvertBounds;

            return isLayOnRay && isInBounds;
        }


        public override IntersectState IntersectRay(MathRay otherRay, out Vector2 intersect)
        {
            IntersectState rayState = base.IntersectRay(otherRay, out intersect);

            if (rayState == IntersectState.Intersect)
            {
                bool isInCommonBounds = (point0.x <= intersect.x) && (intersect.x <= point1.x);
                bool isInInvertBounds = (point1.x <= intersect.x) && (intersect.x <= point0.x);
                bool isInBounds = isInCommonBounds || isInInvertBounds;

                if (!isInBounds)
                {
                    rayState = IntersectState.NonIntersect;
                }
            }

            return rayState;
        }


        #endregion
    }
}
