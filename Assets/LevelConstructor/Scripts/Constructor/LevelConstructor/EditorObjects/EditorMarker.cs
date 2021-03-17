using Drawmasters.Levels;
using Drawmasters.Levels.Objects;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorMarker : EditorLevelObject
    {
        #region Properties

        public MarkerType Type { get; set; }

        #endregion


        #region Methods

        public override LevelObjectData GetData()
        {
            LevelObjectData data = base.GetData();

            Marker.SerializableData markerData = new Marker.SerializableData();
            markerData.type = Type;

            data.additionalInfo = JsonUtility.ToJson(markerData);

            return data;
        }


        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            if (!string.IsNullOrEmpty(data.additionalInfo))
            {
                Marker.SerializableData markerData = JsonUtility.FromJson<Marker.SerializableData>(data.additionalInfo);

                Type = markerData.type;
            }
        }


        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("MarkerInspectorExtension.prefab");

        #endregion
    }
}
