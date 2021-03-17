using UnityEngine;


namespace Drawmasters.Utils.Ui
{
    // Borrowed from pinatamasters project
    public static class SafeOffset
    {
        public static float GetSafeTopRatio(Rect canvasRect)
        {
            Rect safeArea = Screen.safeArea;
            //safeArea = new Rect(0, 102, 1125, 2202); uncomment if you'd like to test IPhoneX

            float areaHeight = canvasRect.height * safeArea.yMax / Screen.height;
            return (canvasRect.height - areaHeight) / canvasRect.height;
        }


        public static float GetSafeTopWithBannerRatio(Rect canvasRect)
        {
            float areaHeight = canvasRect.height * Screen.safeArea.yMax / Screen.height;
            float bannerHeight = 180f * canvasRect.height / Screen.height;

            return (canvasRect.height - areaHeight + bannerHeight) / canvasRect.height;
        }
    }
}