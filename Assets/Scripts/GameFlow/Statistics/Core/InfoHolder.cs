using System;
using System.Collections.Generic;


namespace Drawmasters.Prefs
{
    public class InfoHolder<T, V>
        where T : HoldInfo<V>, new()
        where V : struct, IConvertible
    {
        #region Fields

        private readonly List<T> holdingInfo;
        private readonly string prefsKey;

        #endregion



        #region Lifecycle

        public InfoHolder(string _prefsKey)
        {
            prefsKey = _prefsKey;

            holdingInfo = CustomPlayerPrefs.GetObjectValue<List<T>>(prefsKey);
            if (holdingInfo == null)
            {
                holdingInfo = new List<T>();

                SaveData();
            }
        }

        #endregion



        #region Methods

        public void SetData(V key, T info)
        {
            int findedIndex = holdingInfo.FindIndex(data => data.key.Equals(key));

            bool isExist = (findedIndex != -1);

            if (isExist)
            {
                holdingInfo[findedIndex] = info;
            }
            else
            {
                holdingInfo.Add(info);
            }

            SaveData();
        }


        public bool GetData(V key, out T result)
        {
            result = holdingInfo.Find(data => data.key.Equals(key));

            return (result != null);
        }


        private void SaveData() => CustomPlayerPrefs.SetObjectValue(prefsKey, holdingInfo);

        #endregion
    }
}

