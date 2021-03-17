using UnityEngine;

namespace Drawmasters.Geometry
{
    public class PointPair
    {
        private Vector2 p0, p1;

        public Vector2 Point0 => p0;

        public Vector2 Point1 => p1;

        public PointPair(Vector2 _p0, Vector2 _p1)
        {
            p0 = _p0;
            p1 = _p1;
        }
    }
}