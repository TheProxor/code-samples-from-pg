using Drawmasters.Proposal.Interfaces;
using System;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSkinPropose
    {
        #region Fields

        [SerializeField] protected GameObject alertImage = default;

        // private AdsSkinPanelsSettings settings;

        protected IAlertable[] alertable;

        #endregion



        #region Properties

        protected bool IsActive => alertable != null && alertable.Any(i => i.CanShowAlert);

        #endregion



        #region Public methods

        public void Initialize()
        {
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        public void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            alertable = null;
        }

        public void SetupSettings(IAlertable alertableObject)
        {
            // settings = _settings;
            alertable = new []{ alertableObject };
        }

        public void SetupSettings(IAlertable[] alertableArray)
        {
            // settings = _settings;
            alertable = alertableArray;
        }


        public void SetupAvailableWeaponType(WeaponType _weaponType)
        {
            // settings.SetAvailableWeaponType(_weaponType);
        }

        #endregion
        
        

        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            CommonUtility.SetObjectActive(alertImage, IsActive);
        }

        #endregion
    }
}
