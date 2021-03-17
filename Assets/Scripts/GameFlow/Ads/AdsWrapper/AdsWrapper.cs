using System;
using Modules.General.Abstraction;


namespace Drawmasters.Advertising
{
    public class AdsWrapper
    {
        #region Fields

        protected static string videoAvailabilityKey = "VideoAvailability";
        protected static string interstitialAvailabilityKey = "InterstitialAvailabilityKey";
        
        protected static string instantResultType = "InstantResultType";

        #endregion



        #region Properties

        public static bool IsAvailableForVideo
        {
            get => CustomPlayerPrefs.GetBool(videoAvailabilityKey);
            set => CustomPlayerPrefs.SetBool(videoAvailabilityKey, value);
        }


        public static bool IsAvailableForInterstitial
        {
            get => CustomPlayerPrefs.GetBool(interstitialAvailabilityKey);
            set => CustomPlayerPrefs.SetBool(interstitialAvailabilityKey, value);
        }


        public static AdActionResultType ResultType
        {
            get => CustomPlayerPrefs.GetEnumValue<AdActionResultType>(instantResultType, AdActionResultType.Success);
            set => CustomPlayerPrefs.SetEnumValue(instantResultType, value);
        }

        #endregion



        #region Api

        public bool IsAvailableForModule(AdModule module) =>
            IsAvailableForInterstitial || IsAvailableForVideo;
        

        public void ShowAdsTryShowAdByModule(AdModule module,
            string adShowingPlacement, Action<AdActionResultType> callback = null)
        {
            if (IsAvailableForModule(module))
            {
                callback?.Invoke(ResultType);
            }
        }

        #endregion
    }
}