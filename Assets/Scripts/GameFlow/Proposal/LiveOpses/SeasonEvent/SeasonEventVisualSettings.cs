using System;
using UnityEngine;
using Spine.Unity;
using System.Linq;
using Drawmasters.Announcer;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "SeasonEventVisualSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "SeasonEventVisualSettings")]
    public class SeasonEventVisualSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class HeaderData
        {
            public PetSkinType petSkinType = default;
            public string headerText = default;
            public SkeletonDataAsset previewSkeletonGraphic = default;
            public Sprite commonRewardImage = default;
        }

        #endregion



        #region Fields

        [Header("Animations")]
        public FactorAnimation materialBlendAnimation = default;
        public ColorAnimation plankGraphicColorAnimation = default;

        [Header("Icon")]
        public Sprite barBonusLevelSprite = default;
        public FactorAnimation iconHideAlphaAnimation = default;

        public FactorAnimation iconShowAlphaAnimation = default;
        public VectorAnimation iconShowScaleAnimationIn = default;
        public VectorAnimation iconShowScaleAnimationOut = default;

        [Header("Visual")]

        [SerializeField] private HeaderData[] headerData = default;
        public Sprite commonRewardSprite = default;
        public string commonRewardHeaderKey = default;
        [SerializeField] private SeasonEventStateData.LevelElement[] uiLevelElementsData = default;
        [SerializeField] private SeasonEventStateData.UiElement[] uiElementsStateData = default;

        [SerializeField] private SeasonEventStateData.Currency[] currencyIconData = default;
        [SerializeField] private SeasonEventStateData.PetSkin[] petSkinsIconData = default;

        public string plankLocalizationKey = default;

        [Header("Preview Screen")]
        public Sprite previewGemsSprite = default;

        [Header("Screen Animation")]
        public ColorAnimation shineAlfaAnimation = default;
        public VectorAnimation laspScaleAnimation = default;
        public FactorAnimation fillBarAnimation = default;
        public VectorAnimation barTrailAnimation = default;

        public VectorAnimation lockLineMoveAnimation = default;

        [Header("Time line delays for convenient changes")]

        [SerializeField] private float tutorialDelayOffset = default;
        public float rewardElementTutorialDelay = default;

        public float tutorialStartDelayAfterBarFilled = default;
        [SerializeField] private float elementsStateAnimationDelayAfterBarFilled = default;

        [SerializeField] private float lockLineAnimationDelayAfterBarFilled = default;
        [SerializeField] private float progressBarResumeDelayAfterBarFilled = default;

        [SerializeField] private float lockLineMoveDelayAfterAnimation = default;

        [Header("Tutorial")]
        public FactorAnimation tutorialAlphaAnimation = default;

        [Header("Scroll")]
        public Vector2 scrollContentButtonUpThreshold = default;

        [Header("Season pass visual")]
        public string seasonPassAdsHeaderKey = default;
        public string seasonPassIapHeaderKey = default;

        #endregion



        #region Methods

        public Sprite FindLevelElementSprite(int index)
        {
            var reachedData = Array.FindAll(uiLevelElementsData, e => e.minIndexForSprite <= index).OrderBy(e => e.minIndexForSprite).LastOrDefault();
            return reachedData == null ? default : reachedData.sprite;
        }


        public bool FindIfLevelElementNextStage(int index) =>
          Array.FindAll(uiLevelElementsData, e => e.minIndexForSprite == index).OrderBy(e => e.minIndexForSprite).Any();


        public Color FindLevelElementOutlineColor(int index)
        {
            var reachedData = Array.FindAll(uiLevelElementsData, e => e.minIndexForSprite <= index).OrderBy(e => e.minIndexForSprite).LastOrDefault();
            return reachedData == null ? default : reachedData.outlineColor;
        }
        

        public string FindHeaderKey(PetSkinType petSkinType)
        {
            var data = Array.Find(headerData, e => e.petSkinType == petSkinType);
            return data == null ? string.Empty : data.headerText;
        }


        public SkeletonDataAsset GetPreviewSkeletonGraphic(PetSkinType petSkinType)
        {
            var data = Array.Find(headerData, e => e.petSkinType == petSkinType);
            return data == null ? default : data.previewSkeletonGraphic;
        }
        

        public Sprite GetcommonRewardSprite(PetSkinType petSkinType)
        {
            var data = Array.Find(headerData, e => e.petSkinType == petSkinType);
            return data == null ? default : data.commonRewardImage;
        }
        

        public Sprite GetRewardBackSprite(SeasonEventRewardType seasonEventRewardType, UiSeasonEventRewardElement.State state)
        {
            var data = FindUiElementData(seasonEventRewardType);

            if (data == null)
            {
                return default;
            }

            var spriteData = Array.Find(data.stateSpriteData, e => e.state == state);
            return spriteData == null ? default : spriteData.back;
        }


        public Color GetRewardTextOutlineColor(SeasonEventRewardType seasonEventRewardType, UiSeasonEventRewardElement.State state)
        {
            var data = FindUiElementData(seasonEventRewardType);

            if (data == null)
            {
                return default;
            }

            var spriteData = Array.Find(data.stateSpriteData, e => e.state == state);
            return spriteData == null ? default : spriteData.textOutlineColor;
        }

        public Sprite FindCurrencyIcon(SeasonEventRewardType seasonEventRewardType, CurrencyType currencyType, UiSeasonEventRewardElement.State state)
        {
            var data = FindCurrencyIconData(seasonEventRewardType, currencyType);
            if (data == null)
            {
                return default;
            }

            var spriteData = Array.Find(data.stateSpriteData, e => Array.Exists(e.states, s => s == state));
            return spriteData == null ? default : spriteData.sprite;
        }


        public Sprite FindPetSkinIcon(SeasonEventRewardType seasonEventRewardType, PetSkinType petSkin, UiSeasonEventRewardElement.State state)
        {
            var data = FindPetSkinData(seasonEventRewardType, petSkin);
            if (data == null)
            {
                return default;
            }

            var spriteData = Array.Find(data.stateSpriteData, e => Array.Exists(e.states, s => s == state));
            return spriteData == null ? default : spriteData.sprite;
        }

        public Sprite FindWeaponSkinIcon(WeaponSkinType skinType) =>
            IngameData.Settings.weaponSkinSettings.GetSkinUiSprite(skinType);


        public Sprite FindShooterSkinIcon(ShooterSkinType skinType) =>
            IngameData.Settings.shooterSkinsSettings.GetSkinUiSprite(skinType);


        public Sprite FindAnnouncerBackgroundSprite(SeasonEventRewardType seasonEventRewardType)
        {
            var data = FindUiElementData(seasonEventRewardType);
            return data?.announcerBackgroundSprite;
        }


        public CommonAnnouncer.Data FindAnnouncerData(SeasonEventRewardType seasonEventRewardType)
        {
            var data = FindUiElementData(seasonEventRewardType);
            return data?.announcerData;
        }


        public Vector3 FindAnnouncerOffset(SeasonEventRewardType seasonEventRewardType)
        {
            var data = FindUiElementData(seasonEventRewardType);
            return data?.canNotClaimAnnouncerMoveOffset ?? default;
        }


        public Sprite FindForcemeterRewardIcon(SeasonEventRewardType seasonEventRewardType, UiSeasonEventRewardElement.State state)
        {
            var data = FindUiElementData(seasonEventRewardType);
            var spriteData = data == null ? default : Array.Find(data.stateSpriteData, e => e.state == state);
            
            return spriteData?.forcemeterRewardSprite;
        }

        public Sprite FindSpinRouletteCashRewardIcon(SeasonEventRewardType seasonEventRewardType, UiSeasonEventRewardElement.State state)
        {
            var data = FindUiElementData(seasonEventRewardType);
            var spriteData = data == null ? default : Array.Find(data.stateSpriteData, e => e.state == state);
            
            return spriteData?.spinRouletteCashRewardSprite;
        }


        public Sprite FindSpinRouletteSkinRewardIcon(SeasonEventRewardType seasonEventRewardType, UiSeasonEventRewardElement.State state)
        {
            var data = FindUiElementData(seasonEventRewardType);
            var spriteData = data == null ? default : Array.Find(data.stateSpriteData, e => e.state == state);
            
            return spriteData?.spinRouletteSkinRewardSprite;
        }


        public Sprite FindSpinRouletteWeaponRewardIcon(SeasonEventRewardType seasonEventRewardType, UiSeasonEventRewardElement.State state)
        {
            var data = FindUiElementData(seasonEventRewardType);

            if (data == null)
            {
                return default;
            }

            var spriteData = Array.Find(data.stateSpriteData, e => e.state == state);
            return spriteData?.spinRouletteWaiponRewardSprite;
        }


        public float GetElementsStateAnimationDelayAfterBarFilled(bool isTutorial) =>
            GetDelayWithOffset(elementsStateAnimationDelayAfterBarFilled, isTutorial);


        public float GetLockLineAnimationDelayAfterBarFilled(bool isTutorial) =>
            GetDelayWithOffset(lockLineAnimationDelayAfterBarFilled, isTutorial);


        public float GetProgressBarResumeDelayAfterBarFilled(bool isTutorial) =>
            GetDelayWithOffset(progressBarResumeDelayAfterBarFilled, isTutorial);


        public float GetLockLineMoveDelayAfterAnimation(bool isTutorial) =>
            GetDelayWithOffset(lockLineMoveDelayAfterAnimation, isTutorial);


        private SeasonEventStateData.Currency FindCurrencyIconData(SeasonEventRewardType seasonEventRewardType, CurrencyType currencyType)
        {
            var foundData = Array.Find(currencyIconData, e => e.type == currencyType && Array.Exists(e.eventRewardtypes, t => t == seasonEventRewardType));
            AssertLog(foundData == null, $"No data found for currencyType {currencyType} and seasonEventRewardType {seasonEventRewardType} in {this}");

            return foundData;
        }


        private SeasonEventStateData.PetSkin FindPetSkinData(SeasonEventRewardType seasonEventRewardType, PetSkinType petSkinType)
        {
            var foundData = Array.Find(petSkinsIconData, e => e.type == petSkinType && e.eventRewardtype == seasonEventRewardType);
            AssertLog(foundData == null, $"No data found for currencyType {petSkinType} and seasonEventRewardType {seasonEventRewardType} in {this}");

            return foundData;
        }


        private SeasonEventStateData.UiElement FindUiElementData(SeasonEventRewardType seasonEventRewardType)
        {
            var data = Array.Find(uiElementsStateData, e => e.eventRewardtype == seasonEventRewardType);
            AssertLog(data == null, $"No data found for seasonEventRewardType {seasonEventRewardType} in {this}");

            return data;
        }


        private float GetDelayWithOffset(float delay, bool isTutorial) =>
            isTutorial ? delay + tutorialDelayOffset : delay;

        private void AssertLog(bool assertCondition, string log)
        {
            if (assertCondition)
            {
                CustomDebug.Log(log);
            }
        }

        #endregion
    }
}
