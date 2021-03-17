using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Mansion;


namespace Drawmasters
{
    [Serializable]
    public class PlayerRoomData
    {
        #region Nested types

        [Serializable]
        public class ObjectData
        {
            public MansionRoomObjectType objectType = default;
            public int upgradesCount = default;
            public bool wasHardEntered = default;
        }

        #endregion



        #region Fields

        public bool wasRoomOpened = default;// to prevent changing from a/b test
        public bool wasRoomCompleted = default;

        public List<ObjectData> objectData = new List<ObjectData>();

        #endregion



        #region Methods

        public MansionRoomObjectType[] GetAvailableObjectsTypes() =>
            objectData.Select(e => e.objectType).ToArray();
        

        public void WriteAvailableObjects(MansionRoomObjectType[] roomObjectsTypes)
        {
            foreach (var type in roomObjectsTypes)
            {
                if (!objectData.Exists(e => e.objectType == type))
                {
                    ObjectData dataToAdd = new ObjectData
                    {
                        objectType = type
                    };

                    objectData.Add(dataToAdd);
                }
            }
        }


        public void MarkRoomCompleted() =>
            wasRoomCompleted = true;

        public void OpenRoom() =>
            wasRoomOpened = true;


        public int FindObjectUpgradesCount(MansionRoomObjectType type)
        {
            ObjectData foundData = objectData.Find(e => e.objectType == type);

            if (foundData == null)
            {
                //TODO discuss
//                CustomDebug.Log($"Object data is null. Type: {type}");
            }

            return foundData == null ? default : foundData.upgradesCount;
        }


        public void IncrementObjectUpgrade(MansionRoomObjectType type)
        {
            if (EnsureObjectExistance(type))
            {
                objectData.Find(e => e.objectType == type).upgradesCount++;
            }
        }


        public void UnmarkAllObjectsUpgradeHardEntered() =>
                objectData.ForEach(e => e.wasHardEntered = false);


        public void MarkObjectUpgradeHardEntered(MansionRoomObjectType type)
        {
            if (EnsureObjectExistance(type))
            {
                objectData.Find(e => e.objectType == type).wasHardEntered = true;
            }
        }


        public bool WasObjectUpgradeHardEntered(MansionRoomObjectType type)
        {
            EnsureObjectExistance(type);
            return objectData.Find(e => e.objectType == type).wasHardEntered;
        }


        private bool EnsureObjectExistance(MansionRoomObjectType type)
        {
            bool exists = objectData.Exists(e => e.objectType == type);
            if (!exists)
            {
                CustomDebug.Log($"No data found for {type} in mansion objects data");
            }

            return exists;

        }

        #endregion
    }
}
