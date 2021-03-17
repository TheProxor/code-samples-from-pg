using System;
using System.Collections.Generic;
using Drawmasters.Mansion;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters.Statistics.Data
{
    public class PlayerMansionData : BaseDataSaveHolder<PlayerMansionData.Data>
    {
        #region Helpers

        [Serializable]
        public class Data : BaseDataSaveHolderData
        {
            public readonly List<PlayerRoomData> mansionRoomsData = new List<PlayerRoomData>();
        }

        #endregion



        #region Fields

        public const int MansionRoomsCount = 4;

        protected override string SaveKey =>
            PrefsKeys.PlayerInfo.PlayerMansionData;

        #endregion



        #region Class lifecycle

        public PlayerMansionData() : base()
        {
            int missedRoomsCount = MansionRoomsCount - data.mansionRoomsData.Count;
            for (int i = 0; i < missedRoomsCount; i++)
            {
                data.mansionRoomsData.Add(new PlayerRoomData());
            }

            SaveData();
        }

        #endregion



        #region Methods


        public PlayerRoomData FindMansionRoomData(int roomIndex)
        {
            if (EnsureMansionRoomDataExists(roomIndex))
            {
                return data.mansionRoomsData[roomIndex];
            }
            else
            {
                Debug.Break();
                return default;
            }
        }

        public void MarkRoomCompleted(int roomIndex)
        {
            if (EnsureMansionRoomDataExists(roomIndex))
            {
                data.mansionRoomsData[roomIndex].MarkRoomCompleted();

                SaveData();
            }
        }


        public bool WasRoomCompleted(int roomIndex)
        {
            if (EnsureMansionRoomDataExists(roomIndex))
            {
                return data.mansionRoomsData[roomIndex].wasRoomCompleted;
            }

            return default;
        }



        public void OpenRoom(int roomIndex)
        {
            if (EnsureMansionRoomDataExists(roomIndex))
            {
                data.mansionRoomsData[roomIndex].OpenRoom();

                SaveData();
            }
        }


        public bool WasRoomOpened(int roomIndex)
        {
            if (EnsureMansionRoomDataExists(roomIndex))
            {
                return data.mansionRoomsData[roomIndex].wasRoomOpened;
            }
            else
            {
                Debug.Break();
                return false;
            }
        }


        public int FindMansionRoomObjectUpgrades(int roomIndex, MansionRoomObjectType objectType)
        {
            if (EnsureMansionRoomDataExists(roomIndex))
            {
                return data.mansionRoomsData[roomIndex].FindObjectUpgradesCount(objectType);
            }

            return default;
        }


        public void UpgradeMansionRoomObject(int roomIndex, MansionRoomObjectType objectType)
        {
            if (EnsureMansionRoomDataExists(roomIndex))
            {
                data.mansionRoomsData[roomIndex].IncrementObjectUpgrade(objectType);

                SaveData();
            }
        }


        private bool EnsureMansionRoomDataExists(int roomIndex)
        {
            bool isDataExists = roomIndex < data.mansionRoomsData.Count;

            if (!isDataExists)
            {
                CustomDebug.Log($"No data for index {roomIndex} exists. Wrong access");
            }

            return isDataExists;
        }

        #endregion
    }
}
