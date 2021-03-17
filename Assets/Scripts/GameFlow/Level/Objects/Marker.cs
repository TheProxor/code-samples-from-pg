using System;
using UnityEngine;


namespace Drawmasters.Levels.Objects
{
    public enum MarkerType
    {
        None = 0
    }

    public class Marker : LevelObject
    {
        [Serializable]
        public struct SerializableData
        {
            public MarkerType type;
        }


        public MarkerType Type { get; private set; }


        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            SerializableData markerData = JsonUtility.FromJson<SerializableData>(data.additionalInfo);
            Type = markerData.type;
        }
    }
} 
