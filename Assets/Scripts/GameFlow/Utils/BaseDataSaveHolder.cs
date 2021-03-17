using System;
using System.Collections.Generic;


namespace Drawmasters.Utils
{
    [Serializable]
    public class BaseDataSaveHolderData { }


    [Serializable]
    public abstract class BaseDataSaveHolder<D> where D : BaseDataSaveHolderData, new()
    {
        #region Fields

        protected readonly D data;

        #endregion



        #region Properties

        protected abstract string SaveKey { get; }

        #endregion



        #region Class lifecycle

        public BaseDataSaveHolder()
        {
            data = CustomPlayerPrefs.GetObjectValue<D>(SaveKey);

            data = data ?? new D();

            SaveData();
        }

        #endregion



        #region Methods

        protected void SaveData() =>
            CustomPlayerPrefs.SetObjectValue(SaveKey, data);

        #endregion
    }
}