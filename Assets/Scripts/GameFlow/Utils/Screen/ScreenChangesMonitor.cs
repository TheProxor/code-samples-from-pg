// almost the whole logic was copypasted from https://forum.unity.com/threads/device-screen-rotation-event.118638/
using System;
using System.Collections;
using UnityEngine;


namespace Drawmasters.Utils
{
    public static class ScreenChangesMonitor
    {
        public static event Action<Vector2> OnResolutionChange;
        public static event Action<DeviceOrientation> OnOrientationChange;
        public static float CheckDelay = 0.5f;        // How long to wait until we check again.

        static Vector2 resolution;                    // Current Resolution
        static DeviceOrientation orientation;        // Current Device Orientation
        static Coroutine monitorRoutine;

        private static WaitForSeconds waitForSeconds;


        public static void Initialize()
        {
            resolution = new Vector2(Screen.width, Screen.height);
            orientation = Input.deviceOrientation;

            waitForSeconds = new WaitForSeconds(CheckDelay);
            monitorRoutine = MonoBehaviourLifecycle.PlayCoroutine(CheckForChange());
        }


        public static void Deinitialize() =>
            MonoBehaviourLifecycle.StopPlayingCorotine(monitorRoutine);


        private static IEnumerator CheckForChange()
        {
            // Check for a Resolution Change
            if (resolution.x != Screen.width || resolution.y != Screen.height)
            {
                resolution = new Vector2(Screen.width, Screen.height);
                OnResolutionChange?.Invoke(resolution);
            }

            // Check for an Orientation Change
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.Unknown:            // Ignore
                case DeviceOrientation.FaceUp:            // Ignore
                case DeviceOrientation.FaceDown:        // Ignore
                    break;
                default:
                    if (orientation != Input.deviceOrientation)
                    {
                        orientation = Input.deviceOrientation;
                        OnOrientationChange?.Invoke(orientation);
                    }
                    break;
            }

            yield return waitForSeconds;
        }
    }
}
