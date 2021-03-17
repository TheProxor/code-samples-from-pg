/*
 * Almost the all code was borrowed from Pinatamasters project
 * https://bitbucket.org/playgendary-dev/pinatamasters-unity/src/develop/Assets/Scripts/GameFlow/Utils/TapZone.cs
 */
using System;
using UnityEngine;


namespace Drawmasters.Helpers
{
    [RequireComponent(typeof(Collider2D))]
    public class IngameTouchMonitor : MonoBehaviour
    {
        #region Variables

        public static event Action OnAnyTap = delegate { };
        public static event Action OnAnyUp = delegate { };

        public static bool IsHolding;

        public event Action OnTap = delegate { };
        public event Action OnUp = delegate { };

        private bool isLocked = false;

        #endregion



        #region Public methods

        public void Lock(bool value)
        {
            isLocked = value;
        }

        #endregion



        #region Private methods

        private void OnMouseDown()
        {
            if (!isLocked)
            {
                OnTap();
                OnAnyTap();

                IsHolding = true;
            }
        }


        private void OnMouseUp()
        {
            if (!isLocked)
            {
                OnUp();
                OnAnyUp();

                IsHolding = false;
            }
        }

        #endregion
    }
}
