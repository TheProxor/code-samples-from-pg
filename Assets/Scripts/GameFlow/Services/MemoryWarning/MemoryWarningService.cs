using System;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;


namespace Drawmasters.ServiceUtil
{
    public class MemoryWarningService : IMemoryWarning
    {
        #region IMemoryWarning

        public event Action OnApplicationMemoryWarning;

        #endregion



        #region Ctor

        public MemoryWarningService()
        {
            LLMemoryWarningManager.RegisterMemoryWarning(MemoryWarningCallback);
        }

        #endregion



        #region Events handlers

        private void MemoryWarningCallback()
        {
            Debug.Log("Memory warning callback was invoked.");
            
            OnApplicationMemoryWarning?.Invoke();
            
            CustomPlayerPrefs.Save();
        }

        #endregion
    }
}