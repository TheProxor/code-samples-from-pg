using UnityEngine;

namespace Drawmasters.Geometry
{
    public static class MathUtility
    {
        public static bool IsCounterClockwise(PointPair p0,
                                              PointPair p1,
                                              out float angle)
        {
            bool isCounterClockwise;

            Vector2 d = p0.Point1 - p0.Point0;
            Vector2 dd = p1.Point1 - p1.Point0;

            angle = Vector2.SignedAngle(d, dd);            

            if (angle > 0f)
            {
                isCounterClockwise = angle < 180f;
            }
            else
            {
                isCounterClockwise = angle < -180f;
            }

            return isCounterClockwise;
        }


        public static bool IsRaysIntersect(PointPair seg0,
                                           PointPair seg1,
                                           out Vector2 intersect)
        {
            MathRay ray0 = new MathRay(seg0.Point0, seg0.Point1);
            MathRay ray1 = new MathRay(seg1.Point0, seg1.Point1);

            bool result = ray0.IntersectRay(ray1, out intersect) == IntersectState.Intersect;

            return result;
        }


        public static IntersectState RaysIntersect(PointPair seg0,
                                                   PointPair seg1,
                                                   out Vector2 intersect)
        {
            MathRay ray0 = new MathRay(seg0.Point0, seg0.Point1);
            MathRay ray1 = new MathRay(seg1.Point0, seg1.Point1);

            IntersectState result = ray0.IntersectRay(ray1, out intersect);

            return result;
        }

        public static float PositiveAngle(PointPair p) => PositiveAngle(p.Point0, p.Point1);


        public static float PositiveAngle(Vector2 v0, Vector2 v1) => PositiveAngle(v1 - v0);


        public static float PositiveAngle(Vector2 v)
        {
            float result = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                        
            return PositiveAngle(result);
        }

        public static float PositiveAngle(float angle)
        {
            angle += 360f;
            angle %= 360f;

            return angle;
        }


        public static bool InRange(float minBorder, float maxBorder, float value) =>
            value >= minBorder && value <= maxBorder;
    }
}
