using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    [Serializable]
    public class LevelObjectData
    {
        [Header("Common data")]
        public int index = default;

        public Vector3 position = default;
        public Vector3 rotation = default;
        public bool isLockZ = default;
        public LevelObjectMoveSettings moveSettings = default;
        public List<Vector3> jointsPoints = default;
        public string additionalInfo = default;
        public string colorsInfo = default;

        // field predominantly used for linked spikes. Placed here to not break json in additionalinfo 
        public bool isLinkedObjectsPart = default;

        [Header("Stages data (Boss only)")]
        public int createdStageIndex = default;
        public bool shouldPlayEffectsOnPush = default;
        public List<StageLevelObjectData> stageData = default;

        [Header("Bonus level settings")] public BonusLevelObjectData bonusData = default;

        public LevelObjectData Copy()
        {
            LevelObjectData result = (LevelObjectData)MemberwiseClone();
            result.moveSettings = new LevelObjectMoveSettings(moveSettings);
            result.jointsPoints = new List<Vector3>(jointsPoints);

            return result;
        }


        public LevelObjectData(){}


        public LevelObjectData(int index,
                               Vector3 position, 
                               Vector3 rotation, 
                               bool isLockZ, 
                               LevelObjectMoveSettings moveSettings, 
                               List<Vector3> _jointsPoints,
                               string additionalInfo)
        {
            this.index = index;
            this.position = position;
            this.rotation = rotation;
            this.isLockZ = isLockZ;
            this.moveSettings = moveSettings;
            jointsPoints = _jointsPoints;
            this.additionalInfo = additionalInfo;
        }
        
    }
}

