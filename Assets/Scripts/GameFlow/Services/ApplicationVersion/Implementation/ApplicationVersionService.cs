using System;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;


namespace Drawmasters.ServiceUtil
{
    public class ApplicationVersionService : IApplicationVersion
    {
        #region IApplicationVersion

        public event Action<string> OnApplicationVersionChanged;

        #endregion



        #region Ctor

        public ApplicationVersionService()
        {
            CheckApplicationVersion();
        }

        #endregion



        #region Private methods

        private void CheckApplicationVersion()
        {
            string savedVersion = CustomPlayerPrefs.GetString(PrefsKeys.Application.ApplicationVersion);

            if (!string.IsNullOrEmpty(savedVersion))
            {
                string currentVersion = Application.version; 
                if (currentVersion != savedVersion)
                {
                    CustomPlayerPrefs.SetString(PrefsKeys.Application.ApplicationVersion, currentVersion);

                    OnApplicationVersionChanged?.Invoke(currentVersion);
                }
            }
            else
            {
                CustomPlayerPrefs.SetString(PrefsKeys.Application.ApplicationVersion, Application.version);
            }
        }

        #endregion
    }
}