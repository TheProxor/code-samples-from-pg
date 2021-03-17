using Drawmasters.ServiceUtil;


namespace Drawmasters.Advertising
{
    public static class CustomPlacementsHolder
    {
        #region Fields

        public static bool wasLoaderHidden;

        #endregion

        #region Ctor

        static CustomPlacementsHolder()
        {
            ApplicationManager.OnApplicationStarted += () => wasLoaderHidden = true;
        }

        #endregion



        #region Custom placements

        public static bool IsBetweenSublevelsInterstitialAvailable(string placementName) =>
            GameServices.Instance.AbTestService.AdsData.isNeedInterstitialsBetweenLevels;

        public static bool WasLoaderHidden(string placementName) => 
            wasLoaderHidden;

        #endregion
    }
}
