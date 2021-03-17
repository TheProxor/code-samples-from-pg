using System;
using UnityEngine;
using UnityEngine.U2D;


namespace Drawmasters.Levels
{
    [Serializable]
    public class PointData
    {
        public int pointIndex = default;
        public Vector3 pointPosition = default;
        public ShapeTangentMode shapeTangentMode = default;


        public PointData(int pointIndex, Vector3 pointPosition, ShapeTangentMode shapeTangentMode)
        {
            this.pointIndex = pointIndex;
            this.pointPosition = pointPosition;
            this.shapeTangentMode = shapeTangentMode;
        }


        public PointData()
        {

        }
    }
}
