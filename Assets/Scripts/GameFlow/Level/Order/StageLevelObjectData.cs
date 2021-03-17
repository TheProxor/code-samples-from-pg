using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    [Serializable]
    public class StageLevelObjectData
    {
        public bool isFreeFall = default;
        public float comeVelocity = default;

        public Vector3 position = default;
        public Vector3 rotation = default;

        public LevelObjectMoveSettings moveSettings = default;
        public List<Vector3> jointsPoints = default;
        public string additionalInfo = default;

        // field predominantly used for linked spikes. Placed here to not break json in additionalinfo 
        public bool isLinkedObjectsPart = default;

        public StageLevelObjectData Copy()
        {
            StageLevelObjectData result = (StageLevelObjectData)MemberwiseClone();
            result.moveSettings = new LevelObjectMoveSettings(moveSettings);
            result.jointsPoints = new List<Vector3>(jointsPoints);

            return result;
        }


        public StageLevelObjectData() { }


        public StageLevelObjectData(Vector3 _position,
                                   Vector3 _rotation,
                                   LevelObjectMoveSettings _moveSettings,
                                   List<Vector3> _jointsPoints,
                                   string _additionalInfo)
        {
            position = _position;
            rotation = _rotation;
            moveSettings = _moveSettings;
            jointsPoints = _jointsPoints;
            additionalInfo = _additionalInfo;
        }


        public StageLevelObjectData(LevelObjectData levelObjectData)
        {
            position = levelObjectData.position;
            rotation = levelObjectData.rotation;
            moveSettings = levelObjectData.moveSettings;

            jointsPoints = (levelObjectData.jointsPoints == null) ?
                new List<Vector3>() : new List<Vector3>(levelObjectData.jointsPoints);

            additionalInfo = levelObjectData.additionalInfo;
        }
    }
}
