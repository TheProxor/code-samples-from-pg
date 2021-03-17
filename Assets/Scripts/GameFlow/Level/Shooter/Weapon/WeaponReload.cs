using System;
using System.Collections;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class WeaponReload
    {
        #region Events

        public static event Action OnWeaponReloadBegin;
        public static event Action OnWeaponReloadEnd;

        #endregion



        #region Fields

        private float reloadTime;

        private Action onReloaded;

        private Coroutine reloadingCoroutine;
        
        #endregion



        #region Properties

        public bool IsReloadingNow { get; private set; }

        #endregion



        #region Ctor

        public WeaponReload(float _reloadTime)
        {
            reloadTime = _reloadTime;
        }

        #endregion



        #region Methods

        public void ReloadWeapon(Action _onReloaded)
        {
            MonoBehaviourLifecycle.StopPlayingCorotine(reloadingCoroutine);

            IsReloadingNow = true;

            reloadingCoroutine = MonoBehaviourLifecycle.PlayCoroutine(ReloadWeaponSequence(_onReloaded));
        }

        #endregion


        #region IEnumerator

        IEnumerator ReloadWeaponSequence(Action onReloaded)
        {
            yield return null; // to process PRojectilesCount event firstly

            OnWeaponReloadBegin?.Invoke();

            yield return new WaitForSecondsRealtime(reloadTime);

            onReloaded?.Invoke();
            IsReloadingNow = false;

            OnWeaponReloadEnd?.Invoke();

            yield break;
        }

        #endregion
    }
}

