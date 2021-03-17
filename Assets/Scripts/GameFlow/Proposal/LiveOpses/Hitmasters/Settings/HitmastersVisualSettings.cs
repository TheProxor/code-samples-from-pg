using System;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "HitmastersVisualSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "HitmastersVisualSettings")]
    public partial class HitmastersVisualSettings : ScriptableObject
    {
        #region Fields

        [Header("Visual")]
        [SerializeField] private GameModeSprite[] gameModeSprites = default;
        [SerializeField] private MapPointIconData[] mapPointIcons = default;
        [SerializeField] private CurrencySkinsIconData[] lapsCurrencyIconData = default;

        public Color mapPointShadowForestColor = default;
        public Color mapPointShadowCanyonColor = default;

        public FactorAnimation materialBlendAnimation = default;
        public ColorAnimation materialTextAnimation = default;
        public VectorAnimation activePointBounceAnimation = default;

        public VectorAnimation finishLineMoveAnimation = default;

        public float playUnlockedAnimationDelay = default;

        public VectorAnimation swipeAllMapAnimation = default;
        public VectorAnimation swipePointsAnimation = default;

        public float rewardPopupTimeout = default;

        [Header("Preview Popup")]
        public SkeletonDataAsset asset = default;
        [SpineBone(dataField = "asset")] public string noriShotBone = default;
        [SpineEvent(dataField = "asset")] public string noriShotEvent = default;

        public RectTransform noriProjectilePrefab = default;
        public Vector2[] noriProjectileShotDirections = default;
        public float noriProjectileSpeed = default;
        public VectorAnimation noriProjectileShotAnimation = default;

        #endregion



        #region Public methods

        public Sprite FindCurrencyIcon(CurrencyType type)
        {
            CurrencySkinsIconData foundData = FindCurrencyIconData(type);
            
            return foundData?.activeIcon;
        }
        
        public (ShooterSkinType type, SkeletonDataAsset asset, string startAnimation, string loopAnimation) FindGameModeSkinAnimationData(GameMode mode)
        {
            (ShooterSkinType type, SkeletonDataAsset asset, string startAnimation, string loopAnimation) result = (default, default, default, default);
            
            GameModeSprite found = Array.Find(gameModeSprites, e => e.mode == mode);
            
            AssertLog(found == null, $"No skinType found for game mode {mode} in {this}");

            if (found != null)
            {
                result.type = found.skinType;
                result.asset = found.asset;
                result.startAnimation = found.startAnimation;
                result.loopAnimation = found.loopAnimation;
            }

            return result;
        }


        public string FindPopupIdleFxKey(GameMode mode)
        {
            GameModeSprite foundSprite = Array.Find(gameModeSprites, e => e.mode == mode);
            
            AssertLog(foundSprite == null, $"No sprite found for game mode {mode} in {this}");

            return foundSprite?.popUpIdleFxKey;
        }


        public EventFxData[] FindPopupEventFxData(GameMode mode)
        {
            GameModeSprite foundSprite = Array.Find(gameModeSprites, e => e.mode == mode);
            AssertLog(foundSprite == null, $"No sprite found for game mode {mode} in {this}");

            return foundSprite?.eventFxData;
        }

        
        public Sprite FindGameModeSprite(GameMode mode)
        {
            Sprite result = default;
            GameModeSprite foundSprite = Array.Find(gameModeSprites, e => e.mode == mode);
            
            AssertLog(foundSprite == null, $"No sprite found for game mode {mode} in {this}");

            if (foundSprite != null)
            {
                result  = foundSprite.sprite;
            }

            AssertLog(result == null, $"No sprite found for game mode {mode} in {this}");

            return result;
        }


        public Sprite FindGameModeSmallSprite(GameMode mode)
        {
            Sprite result = default;
            GameModeSprite foundSprite = Array.Find(gameModeSprites, e => e.mode == mode);
            
            AssertLog(foundSprite == null, $"No sprite found for game mode {mode} in {this}");

            if (foundSprite != null)
            {
                result  = foundSprite.smallSprite;
            }

            AssertLog(result == null, $"No sprite found for game mode {mode} in {this}");

            return result;
        }


        public string FindUiMenuText(GameMode mode)
        {
            GameModeSprite foundSprite = Array.Find(gameModeSprites, e => e.mode == mode);

            AssertLog(foundSprite == null, $"No sprite found for game mode {mode} in {this}");
            
            return foundSprite == null ? string.Empty : foundSprite.uiMenuText;
        }


        public string FindMenuIdleFx(GameMode mode)
        {
            GameModeSprite foundSprite = Array.Find(gameModeSprites, e => e.mode == mode);
            
            AssertLog(foundSprite == null, $"No sprite found for game mode {mode} in {this}");

            return foundSprite?.menuIdleFxKey;
        }

        

        public Sprite FindGameModeRewardSprite(GameMode mode)
        {
            Sprite result = default;
            GameModeSprite foundSprite = Array.Find(gameModeSprites, e => e.mode == mode);
            
            AssertLog(foundSprite == null, $"No sprite found for game mode {mode} in {this}");

            if (foundSprite != null)
            {
                result  = foundSprite.rewardSprite;
            }

            AssertLog(result == null, $"No sprite found for game mode {mode} in {this}");

            return result;
        }

        public string FindUnlockFxKey(HitmastersMapPoint.MapPointType type)
        {
            MapPointIconData foundData = FindMapPointIconData(type);
            
            return foundData == null ? string.Empty : foundData.unlockFxKey;
        }

        public Vector3 FindUnlockFxKeyScale(HitmastersMapPoint.MapPointType type)
        {
            MapPointIconData foundData = FindMapPointIconData(type);
            return foundData?.unlockFxKeyScale ?? Vector3.one;
        }
        

        public Sprite FindMapPointActiveIcon(HitmastersMapPoint.MapPointType type)
        {
            MapPointIconData foundData = FindMapPointIconData(type);
            return foundData?.activeIcon;
        }
        
        public Sprite FindMapPointDisableIcon(HitmastersMapPoint.MapPointType type)
        {
            MapPointIconData foundData = FindMapPointIconData(type);
            return foundData?.disableIcon;
        }

        public Sprite FindMapPointLockedIcon(HitmastersMapPoint.MapPointType type)
        {
            MapPointIconData foundData = FindMapPointIconData(type);
            return foundData?.lockedIcon;
        }

        public Sprite FindMapPointShadowIcon(HitmastersMapPoint.MapPointType type)
        {
            MapPointIconData foundData = FindMapPointIconData(type);
            return foundData?.shadowIcon;
        }

        public string FindMapPointActiveFxKey(HitmastersMapPoint.MapPointType type)
        {
            MapPointIconData foundData = FindMapPointIconData(type);
            return foundData == null ? string.Empty : foundData.idleActiveFxKey;
        }


        public string FindMapPointFxKey(HitmastersMapPoint.MapPointType type)
        {
            MapPointIconData foundData = FindMapPointIconData(type);
            return foundData == null ? string.Empty : foundData.idleFxKey;
        }

        #endregion



        #region Private methods

        private CurrencySkinsIconData FindCurrencyIconData(CurrencyType type)
        {
            CurrencySkinsIconData foundData = Array.Find(lapsCurrencyIconData, e => e.type == type);
            
            AssertLog(foundData == null, $"No data found for type {type} in {this}");

            return foundData;
        }
        
        
        private MapPointIconData FindMapPointIconData(HitmastersMapPoint.MapPointType type)
        {
            MapPointIconData result = Array.Find(mapPointIcons, e => e.type == type);
            
            AssertLog(result == null, $"No data found for type {type} in {this}");

            return result;
        }

        #endregion



        #region Debug logic
        
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
