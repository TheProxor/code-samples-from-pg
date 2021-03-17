using UnityEngine;


namespace Drawmasters
{
    public static class ResolutionUtility
    {
        #region Helper classes

        public static class Resolutions
        {
            public static class Xr
            {
                public const int Height = 828;
                public const int Width = 1792;
            }

            public static class Xs
            {
                public const int Height = 1125;
                public const int Width = 2436;
            }

            public static class XsMax
            {
                public const int Height = 1242;
                public const int Width = 2688;
            }
        }

        #endregion


        #region Fields

        public const float CommonIPhoneXOffset = 90.0f;

        static bool? isMonobrowDevice;
        static bool? isIpad;

        #endregion



        #region Properties

        public static bool IsMonobrowDevice
        {
            get
            {
                if (isMonobrowDevice == null)
                {
                    isMonobrowDevice = false;

                #if UNITY_IOS && !UNITY_EDITOR
                    if (!Mathf.Approximately(Screen.safeArea.yMax, Screen.height))
                    {
                        isMonobrowDevice = true;
                    }
                #elif UNITY_EDITOR
                    int currentWidth = Screen.width;
                    int currentHeight = Screen.height;

                    bool isIphoneXr = (currentHeight == Resolutions.Xr.Height && currentWidth == Resolutions.Xr.Width) ||
                                      (currentHeight == Resolutions.Xr.Width && currentWidth == Resolutions.Xr.Height);

                    bool isIphoneXs = (currentHeight == Resolutions.Xs.Height && currentWidth == Resolutions.Xs.Width) ||
                                      (currentHeight == Resolutions.Xs.Width && currentWidth == Resolutions.Xs.Height);

                    bool isIphoneXsMax = (currentHeight == Resolutions.XsMax.Height && currentWidth == Resolutions.XsMax.Width) ||
                                         (currentHeight == Resolutions.XsMax.Width && currentWidth == Resolutions.XsMax.Height);

                    if (isIphoneXr ||
                        isIphoneXs ||
                        isIphoneXsMax)
                    {
                        isMonobrowDevice = true;
                    }
                #endif
                }

                return isMonobrowDevice.Value;
            }
        }


        public static bool IsIpad
        {
            get
            {
                if (isIpad == null)
                {
                    bool result = false;

                    #if UNITY_IOS
                        float ratio = (float)Screen.height / (float)Screen.width;

                        float maxRatio = 2388f / 1668f; //IPad PRO 11'
                        float minRatio = 1024f / 768f; 

                        result = (ratio >= minRatio && ratio <= maxRatio);
                    #endif

                    isIpad = result;
                }

                return isIpad.Value;
            }
        }

        public static Rect IPhone7SafeArea => new Rect(0.0f, 102.0f, 1125.0f, 2202.0f);

        public static Rect IPhoneXSafeArea => new Rect(0.0f, 102.0f, 1125.0f, 2202.0f);

        #endregion
    }
}
