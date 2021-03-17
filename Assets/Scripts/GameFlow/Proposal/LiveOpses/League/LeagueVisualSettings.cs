using System;
using UnityEngine;
using Drawmasters.Levels;
using Spine.Unity;
using SelectorKey = Drawmasters.Ui.UiLeagueLeaderBoardElementSelectorKey;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "LeagueVisualSettings ",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "LeagueVisualSettings")]
    public class LeagueVisualSettings : ScriptableObjectData<LeagueVisualSettings.Data, LeagueType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<LeagueType>
        {
            public Sprite enabledLeagueIconSprite = default;

            public string headerText = default;
            
            [SpineAnimation(dataField = "leaguesIconsDataAsset")] public string showAnimation = default;
            [SpineAnimation(dataField = "leaguesIconsDataAsset")] public string showWhiteAnimation = default;

            [SpineAnimation(dataField = "leaguesIconsDataAsset")] public string idleAnimation = default;
            [SpineAnimation(dataField = "leaguesIconsDataAsset")] public string idleWhiteAnimation = default;

            public UiLeagueLeaderBoardElemenIconsData[] uiLeagueLeaderBoardElemenIconsDatas = default;
        }


        [Serializable]
        public class UiLeagueLeaderBoardElementData
        {
            public SelectorKey key = default;

            [Header("Currency")]
            public UiLeagueLeaderBoardElemenCurrencyData[] uiLeagueLeaderBoardElemenCurrencyData = default;

            [Header("Chest")]
            public UiLeagueLeaderBoardElementChestData[] uiLeagueLeaderBoardElementChestData = default;

            [Header("Skin icons")]
            public ShooterSkinsIconDataBase[] shooterSkinsIconData = default;
            public WeaponSkinsIconDataBase[] weaponSkinsIconData = default;
            public PetSkinsIconDataBase[] petSkinsIconData = default;
        }


        [Serializable]
        public class UiLeagueLeaderBoardElemenCurrencyData
        {
            public CurrencyType currencyType = default;
            public Sprite sprite = default;
        }


        [Serializable]
        public class UiLeagueLeaderBoardElementChestData
        {
            public ChestType chestType = default;
            public Sprite sprite = default;
        }


        [Serializable]
        public class UiLeagueLeaderBoardElemenIconsData
        {
            public LeaderBordItemType ownerType = default;
            public Sprite sprite = default;
        }


        [Serializable]
        public class PlacesData
        {
            public int boardPosition = default;
            public Sprite placeSprite = default;
            public Color outlineColor = default;
        }


        [Serializable]
        public class StartElementData
        {
            public VectorAnimation startAnimation = default;
            public int[] indexesToAnimate = default;

            [Enum(typeof(EffectKeys))] public string onStartFxKey = default;
        }

        #endregion



        #region Fields

        public SkeletonDataAsset leaguesIconsDataAsset = default;
        
        public Sprite noInternetSprite = default;
        
        public UiLeagueLeaderBoardElementData[] uiLeagueLeaderBoardElementData = default;
        
        public UiLeagueLeaderBoardElementData[] uiLeagueEndScreenElementData = default;

        public UiLeagueLeaderBoardElementChestData[] uiLeagueIntermediateMenuChestData = default;
        public UiLeagueLeaderBoardElementChestData[] uiLeagueIntermediateLeaderbordChestData = default;

        public VectorAnimation elementsMoveAnimation = default;
        public FactorAnimation elementsMoveSizeAnimation = default;
        public NumberAnimation elementsSkullsAnimation = default;

        public float startElementsMoveDelay = default;
        public VectorAnimation playerElementMoveScaleAnimation = default;

        public VectorAnimation skullTrailAnimation = default;

        [Header("League change")]
        public VectorAnimation scrollLeagueChangeAnimation = default;
        public FactorAnimation scrollLeagueChangeSwipeAnimation = default;
        public FactorAnimation leagueChangeStrokeAlphaAnimation = default;
        public float leagueIconMinScale = 0.5f;
 
        [Header("League apply")]
        public VectorAnimation enterNameTextScaleAnimation = default;
        public ColorAnimation enterNameTextColorAnimation = default;

        [Header("League end")]
        public VectorAnimation leagueEndElementMoveAnimation = default;
        public FactorAnimation leagueEndElementAlpfaAnimation = default;
        public FactorAnimation leagueEndButtonAlpfaAnimation = default;
        public VectorAnimation leagueEndElementMoveScaleAnimation = default;

        [Header("Localization")]
        public string localizationDescriptionLeaguePrefix = default;
        public string localizationDescriptionKey = default;

        public string petRewardDescriptionKey = default;
        public string gemRewardDescriptionKey = default;

        public string finalRewardTextKey = default;

        [Header("Fonts")]
        [SerializeField] private PlacesData[] placesData = default;

        [Header("Rewards")]
        public Sprite gemsMainRewardSprite = default;
        public FactorAnimation mainRewardFadeAnimation = default;
        public float hideRewardTimeout = default;


        [Header("Scroll")]
        public VectorAnimation startScrollAnimation = default;
        public StartElementData[] startElementData = default;

        [Header("Ui menu")]
        public float timeScaleForProposalAnnouncer = default;

        #endregion



        #region Methods

        public Color FindOutlineColor(int boardPosition)
        {
            PlacesData foundData = Array.Find(placesData, e => e.boardPosition == boardPosition);
            return foundData == null ? Color.white : foundData.outlineColor;
        }


        public Sprite FindPlaceSprite(int boardPosition)
        {
            PlacesData foundData = Array.Find(placesData, e => e.boardPosition == boardPosition);
            return foundData == null ? default : foundData.placeSprite;
        }


        public bool IsPlaceDataExists(int boardPosition) =>
            Array.Exists(placesData, e => e.boardPosition == boardPosition);


        public Sprite FindEnabledLeagueIconSprite(LeagueType leagueType)
        {
            var foundData = FindData(leagueType);
            return foundData == null ? default : foundData.enabledLeagueIconSprite;
        }


        public Sprite FindOfflineLeagueIconSprite()
        {
            return noInternetSprite;
        }


        public string FindHeaderKey(LeagueType leagueType)
        {
            var foundData = FindData(leagueType);
            return foundData == null ? default : foundData.headerText;
        }


        public string FindShowAnimationKey(LeagueType leagueType)
        {
            var foundData = FindData(leagueType);
            return foundData == null ? default : foundData.showAnimation;
        }


        public string FindShowWhiteAnimationKey(LeagueType leagueType)
        {
            var foundData = FindData(leagueType);
            return foundData == null ? default : foundData.showWhiteAnimation;
        }


        public string FindIdleAnimationKey(LeagueType leagueType)
        {
            var foundData = FindData(leagueType);
            return foundData == null ? default : foundData.idleAnimation;
        }


        public string FindIdleWhiteAnimationKey(LeagueType leagueType)
        {
            var foundData = FindData(leagueType);
            return foundData == null ? default : foundData.idleWhiteAnimation;
        }


        public Sprite FindElementLeagueIconSprite(LeagueType leagueType, LeaderBordItemType ownerType)
        {
            var foundData = FindData(leagueType);
            return foundData == null ? default : Array.Find(foundData.uiLeagueLeaderBoardElemenIconsDatas, e => e.ownerType == ownerType).sprite;
        }


        public Sprite FindRewardIconBaseSprite(SelectorKey selectorKey, RewardData rewardData, out float scaleMultiplier)
        {
            Sprite result = FindRewardIconSprite(uiLeagueLeaderBoardElementData, selectorKey, rewardData,
                out scaleMultiplier);
            return result;
        }


        public Sprite FindRewardIconEndScreenSprite(SelectorKey selectorKey, RewardData rewardData, out float scaleMultiplier)
        {
            Sprite result = FindRewardIconSprite(uiLeagueEndScreenElementData, selectorKey, rewardData,
                out scaleMultiplier);
            return result;
        }


        private Sprite FindRewardIconSprite(UiLeagueLeaderBoardElementData[] source, SelectorKey selectorKey, RewardData rewardData, out float scaleMultiplier)
        {
            Sprite result = default;
            scaleMultiplier = 1.0f;

            if (rewardData is CurrencyReward currencyReward)
            {
                result = FindCurrencyTypeIconSprite(source, selectorKey, currencyReward.currencyType);
            }
            else if (rewardData is ShooterSkinReward shooterSkinReward)
            {
                result = FindShooterIconSprite(source, selectorKey, shooterSkinReward.skinType);
            }
            else if (rewardData is WeaponSkinReward weaponSkinReward)
            {
                result = FindWeaponIconSprite(source, selectorKey, weaponSkinReward.skinType);
                scaleMultiplier = 0.7f;
            }
            else if (rewardData is PetSkinReward petSkinReward)
            {
                result = FindPetIconSprite(source, selectorKey, petSkinReward.skinType);
            }
            else if (rewardData is ChestReward chestReward)
            {
                result = FindChestIconSprite(source, selectorKey, chestReward.chestType);
            }
            else
            {
                CustomDebug.Log($"Not implemented logic for {rewardData.Type} in {this}");
            }

            return result;
        }


        public Sprite FindIntermediateMenuChestIcon(ChestType chestType) =>
            FindChestIcon(uiLeagueIntermediateMenuChestData, chestType);


        public Sprite FindIntermediateLeaderbordChestIcon(ChestType chestType) =>
            FindChestIcon(uiLeagueIntermediateLeaderbordChestData, chestType);


        private Sprite FindChestIcon(UiLeagueLeaderBoardElementChestData[] collection, ChestType chestType) =>
            collection.Find(i => i.chestType == chestType).sprite;


        private Sprite FindShooterIconSprite(UiLeagueLeaderBoardElementData[] source, SelectorKey selectorKey, ShooterSkinType shooterSkinType)
        {
            UiLeagueLeaderBoardElementData foundData = FindData(source, selectorKey);
            Sprite targetSprite = foundData == null ? default : Array.Find(foundData.shooterSkinsIconData, e => e.type == shooterSkinType)?.activeIcon;

            if (targetSprite == null)
            {
                CustomDebug.Log($"Can't find shooter skin in {this} for shooterSkinType = {shooterSkinType}." +
                                $" Returning ui common as fallback");
                targetSprite = IngameData.Settings.shooterSkinsSettings.GetSkinUiSprite(shooterSkinType);
            }

            return targetSprite;
        }


        private Sprite FindWeaponIconSprite(UiLeagueLeaderBoardElementData[] source, SelectorKey selectorKey, WeaponSkinType type)
        {
            UiLeagueLeaderBoardElementData foundData = FindData(source, selectorKey);
            Sprite targetSprite = foundData == null ? default : Array.Find(foundData.weaponSkinsIconData, e => e.type == type)?.activeIcon;

            if (targetSprite == null)
            {
                CustomDebug.Log($"Can't find shooter skin in {this} for type = {type}." +
                                $" Returning ui common as fallback");
                targetSprite = IngameData.Settings.weaponSkinSettings.GetSkinUiSprite(type);
            }

            return targetSprite;
        }


        private Sprite FindPetIconSprite(UiLeagueLeaderBoardElementData[] source, SelectorKey selectorKey, PetSkinType type)
        {
            UiLeagueLeaderBoardElementData foundData = FindData(source, selectorKey);
            Sprite targetSprite = foundData == null ? default : Array.Find(foundData.petSkinsIconData, e => e.type == type)?.activeIcon;

            if (targetSprite == null)
            {
                CustomDebug.Log($"Can't find shooter skin in {this} for type = {type}." +
                                $" Returning ui common as fallback");
                targetSprite = IngameData.Settings.pets.skinsSettings.GetSkinUiSprite(type);
            }

            return targetSprite;
        }

        
        private Sprite FindCurrencyTypeIconSprite(UiLeagueLeaderBoardElementData[] source, SelectorKey selectorKey, CurrencyType currencyType)
        {
            UiLeagueLeaderBoardElementData foundData = FindData(source, selectorKey);

            if (foundData == null)
            {
                CustomDebug.Log($"Can't find UiLeagueLeaderBoardElementData in {source} for selector key = {selectorKey}.");
                return default;
            }
            
            UiLeagueLeaderBoardElemenCurrencyData currencyData = 
                Array.Find(foundData.uiLeagueLeaderBoardElemenCurrencyData, e => e.currencyType == currencyType);

            if (currencyData == null)
            {
                CustomDebug.Log($"Can't find UiLeagueLeaderBoardElemenCurrencyData in {foundData} for currency type = {currencyType}.");
                return default;
            }
            
            return  currencyData.sprite;
        }


        private Sprite FindChestIconSprite(UiLeagueLeaderBoardElementData[] source, SelectorKey selectorKey, ChestType chestType)
        {
            UiLeagueLeaderBoardElementData foundData = FindData(source, selectorKey);
            
            if (foundData == null)
            {
                CustomDebug.Log($"Can't find UiLeagueLeaderBoardElementData in {source} for selector key = {selectorKey}.");
                return default;
            }
            
            UiLeagueLeaderBoardElementChestData chestData = 
                Array.Find(foundData.uiLeagueLeaderBoardElementChestData, e => e.chestType == chestType);

            if (chestData == null)
            {
                CustomDebug.Log($"Can't find UiLeagueLeaderBoardElementChestData in {foundData} for chest type = {chestType}.");
                return default;
            }
            
            return  chestData.sprite;
        }


        private UiLeagueLeaderBoardElementData FindData(UiLeagueLeaderBoardElementData[] source, SelectorKey selectorKey)
        {
            UiLeagueLeaderBoardElementData foundData = Array.Find(source, e => e.key.IsEquals(selectorKey));
            AssertLog(foundData == null, $"No data found for type= {selectorKey.type} and isNextLeagueAchived = {selectorKey.isNextLeagueAchived} in {this}");

            return foundData;
        }

        #endregion
    }
}
