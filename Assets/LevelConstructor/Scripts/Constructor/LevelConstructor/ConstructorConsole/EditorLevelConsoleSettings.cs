using System;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    [CreateAssetMenu(fileName = "EditorLevelConsoleSettings",
                     menuName = NamingUtility.MenuItems.ConstructorData + "EditorLevelConsoleSettings")]
    public class EditorLevelConsoleSettings : ScriptableObject
    {
        #region Nested Types

        [Serializable]
        private class LogsInfo
        {
            public ObjectsCollisionType type = default;
            public string color = default;
        }

        #endregion



        #region Fields

        private const string DefaultColorString = "black";

        public float minSpeedValueForLog = default;

        public float fontSize = default;

        [SerializeField] private LogsInfo[] logsInfo = default;

        #endregion



        #region Methods

        public string FindColorTag(ObjectsCollisionType objectsCollisionType)
        {
            string result = DefaultColorString;

            LogsInfo objectCollisionNotifierInfo = Array.Find(logsInfo, element => element.type == objectsCollisionType);

            if (objectCollisionNotifierInfo == null)
            {
                CustomDebug.Log($"No data for type {objectsCollisionType}");
            }
            else
            {
                result = objectCollisionNotifierInfo.color;
            }

            return result;
        }

        #endregion
    }
}
