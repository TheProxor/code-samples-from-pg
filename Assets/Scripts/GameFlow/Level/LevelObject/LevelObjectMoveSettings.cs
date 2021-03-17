using UnityEngine;
using System;
using System.Collections.Generic;


namespace Drawmasters
{
    [Serializable]
    public class LevelObjectMoveSettings
    {
        [Header("Move")]
        public List<Vector3> path = new List<Vector3>();
        public float delayBetweenPoints = default;
        public float totalMoveDuration = default;
        public bool isCycled = default;


        public bool CanMove => path != null && path.Count > 0;


        public float TotalPathDistance
        {
            get
            {
                float result = default;
                Vector3 previousPoint = path.FirstObject();

                foreach (var point in path)
                {
                    result += Vector2.Distance(previousPoint, point);
                    previousPoint = point;
                }

                if (isCycled)
                {
                    result += Vector2.Distance(previousPoint, path.First()); 
                }

                return result;
            }
        }


        public LevelObjectMoveSettings() { }


        public LevelObjectMoveSettings(LevelObjectMoveSettings _moveSettings)
        {
            path = new List<Vector3>(_moveSettings.path);
            delayBetweenPoints = _moveSettings.delayBetweenPoints;
            totalMoveDuration = _moveSettings.totalMoveDuration;
            isCycled = _moveSettings.isCycled;
        }
    }
}
