using UnityEngine;


namespace Drawmasters.Utils
{
    public static class TransformUtility
    {
        #region Fields

        private static Camera UiCamera;
        private static Camera gameCamera;
        
        #endregion



        #region Methods

        public static void Initialize(Camera _UiCamera, Camera _gameCamera)
        {
            UiCamera = _UiCamera;
            gameCamera = _gameCamera;
        }


        public static Vector3 UiToWorldPosition(this Vector3 position)
        {
            Vector3 viewPortUIPosition = UiCamera.WorldToViewportPoint(position);
            Vector3 worldPosition = gameCamera.ViewportToWorldPoint(viewPortUIPosition);

            return worldPosition;
        }


        public static Vector3 WorldToUiPosition(this Vector3 position)
        {
            Vector3 viewportnWorldPosition = gameCamera.WorldToViewportPoint(position);
            Vector3 uiPosition = UiCamera.ViewportToWorldPoint(viewportnWorldPosition);

            return uiPosition;
        }

        #endregion
    }
}
