using System;
using UnityEngine;

namespace Drawmasters.Geometry
{
    public class MathRay
    {
        #region Fields

        protected static float DefautEpsilon = 0.0001f;

        protected float a;
        protected float b;
        protected float c;

        #endregion



        #region Properties

        public float A => a;

        public float B => b;

        public float C => c;

        public virtual float Angle => Mathf.Atan(A / B) * Mathf.Rad2Deg;

        #endregion



        #region Lifecycle

        public MathRay(Vector2 p0, Vector2 p1)
        {
            a = p1.y - p0.y;
            b = p0.x - p1.x;
            c = a * p0.x + b * p0.y;
        }

        #endregion



        #region Methods
               
        public virtual IntersectState IntersectRay(MathRay otherRay, out Vector2 intersect)
        {
            IntersectState result = IntersectState.Intersect;

            float x = 0f, y = 0f;

            float det = A * otherRay.B - otherRay.A * B;

            if (Mathf.Abs(det) < DefautEpsilon)
            {
                result = IntersectState.Parallel;
            }
            else
            {
                x = (otherRay.B * C - B * otherRay.C) / det;
                y = (A * otherRay.C - otherRay.A * C) / det;
            }

            intersect = new Vector2(x, y);

            return result;
        }


        public virtual bool IsPointLay(Vector2 p, float precision)
        {
            float value = A * p.x + B * p.y + C;            

            return (value < precision);
        }


        public bool IsPointLay(Vector2 p) => IsPointLay(p, DefautEpsilon);


        #endregion
    }
}
