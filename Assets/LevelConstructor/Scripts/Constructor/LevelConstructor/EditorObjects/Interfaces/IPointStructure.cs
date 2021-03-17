using Drawmasters.Levels;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public interface IPointStructure
    {
        List<PointData> PointData { get; set; }
        void RefreshPoints(List<PointData> pointsData);
        void RemovePoint(int pointIndex);
        void InsertPoint(int pointIndex, Vector3 pointPosition);
    }
}