using System;
using UnityEngine;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "BonusLevelSettings",
        menuName = NamingUtility.MenuItems.IngameSettings + "BonusLevelSettings")]
    public class BonusLevelSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class VisualSpriteData
        {
            public Sprite sprite = default;
            public Vector3 spriteScale = default;

            public PhysicalLevelObjectType[] type = default;
            public PhysicalLevelObjectShapeType shapeType = default;
            public PhysicalLevelObjectSizeType sizeType = default;
        }


        [Serializable]
        private class VisualFxData
        {
            [Enum(typeof(EffectKeys))]
            public string fxKey = default;

            public PhysicalLevelObjectShapeType[] shapeType = default;
            public PhysicalLevelObjectSizeType[] sizeType = default;
        }


        [Serializable]
        private class DestroyFxData
        {
            public CurrencyType currencyType = default;

            [Enum(typeof(EffectKeys))] public string fxKey = default;
        }

        #endregion



        #region Fields

        [Header("Визуал аутлейна")]
        public float commonOutlineWidth = default;
        public float pinataOutlineWidth = default;

        public Material outlineMaterial = default;
        public Color outlineColor = default;
        public FactorAnimation outlineThresholdAnimation = default;

        [Header("Визуал заморозки")]

        public VectorAnimation spriteScaleAnimation = default;
        public VectorAnimation imageScaleAnimationSubscription = default;

        //TODO: refactoring
        public Sprite diceFrozenSprite = default;

        public Sprite pinataFrozenSprite = default;

        public Sprite petFrozenSprite = default;

        [SerializeField] private VisualSpriteData[] visualSpritesData = default;
        [SerializeField] private VisualFxData[] visualFxData = default;
        [SerializeField] private DestroyFxData[] destroyFxData = default;

        [Enum(typeof(EffectKeys))] public string commonDestroyFx = default;


        [Header("Задержка между началом уровня и спавном объектов")]
        public float spawnDelay = default;
        
        [Header("Задержка перед замедлением")]
        public float decelerationDelay = default;
        
        [Header("Время от начала замедления, через которое объекты остановятся")]
        public float decelerationDuration = default;
        
        [Header("Общая позиция по Y для спавна всех объектов")]
        public float spawnYPosition = default;
        
        [Header("Минимальное значение замедления")]
        public float minTimeScale = default;
        
        [Header("Длительность изменения замедления")]
        public float timeScaleChangeDuration = default;

        [Header("Кривая изменения замедления")]
        public AnimationCurve timeScaleCurve = default;

        [Header("Задержка перед концом уровня")]
        public float levelEndDelay = default;
        

        [Header("Анимация скейла бонусных объектов")]
        public FactorAnimation scaleAnimation = default;

        [Header("Анимация Y позиции текстового аннонсера валюты")]
        public NumberAnimation positionYAnnouncerAnimation = default;

        [Header("Анимация альфы текстового аннонсера валюты")]
        public FactorAnimation alphaAnnouncerAnimation = default;

        [Header("Задержка перед обновлением счетчика")]
        public float delayBeforeCurrencyUpdate = default;

        [Header("Длительность обновления счетчика")]
        public float currencyUpdateDuration = default;

        [Header("Длительность стирания линии")]
        public float lineClearDuration = default;

        [Header("Задержка сбора обязательной награды после замедления")]
        public float mandatoryRewardAutoCollectDelay = default;

        public VectorAnimation projectileDestroyAnimation = default;

        #endregion



        #region Methods

        public float FindOutlideWidth(PhysicalLevelObjectData data, CurrencyType currencyType)
        {
            (Sprite, Vector3) spriteAndScale = FindFrezeeCurrencySpriteAndScale(data, currencyType);
            bool shouldUsePinataSprite = spriteAndScale.Item1 == pinataFrozenSprite; 

            float result = shouldUsePinataSprite ? pinataOutlineWidth : commonOutlineWidth;
            return result;
        }


        public (Sprite, Vector3) FindFrezeeSpriteAndScale(PhysicalLevelObjectData data, BonusLevelObjectData bonusLevelObjectData)
        {
            (Sprite, Vector3) result = (default, default);

            switch(bonusLevelObjectData.rewardType)
            {
                case Proposal.RewardType.Currency:
                    result = FindFrezeeCurrencySpriteAndScale(data, bonusLevelObjectData.currencyType);
                    break;

                case Proposal.RewardType.PetSkin:
                    result = FindFrezeePetSkinSpriteAndScale();
                    break;

                default:
                    CustomDebug.Log($"Can't find freeze sprite for this reward type (<b>{bonusLevelObjectData.rewardType}</b>)");
                    break;
            }

            return result;
        }


        public (Sprite, Vector3) FindFrezeeCurrencySpriteAndScale(PhysicalLevelObjectData data, CurrencyType currencyType)
        {
            (Sprite, Vector3) result = (default, default);

            bool shouldUseDiceSprite = currencyType.IsMonopolyCurrency() && currencyType.IsMonopolyAvailableForShowOnLevel();

            if (shouldUseDiceSprite)
            {
                result.Item1 = diceFrozenSprite;
                result.Item2 = Vector3.one;

                return result;
            }

            VisualSpriteData foundData = Array.Find(visualSpritesData, e => Array.Exists(e.type, t => t == data.type) &&
                                                                     e.shapeType == data.shapeType &&
                                                                     e.sizeType == data.sizeType);
            Log(foundData == null, $"No sprite found for data type = {data.type} shapeType = {data.shapeType} sizeType = {data.sizeType}");

            result.Item1 = foundData == null ? default : foundData.sprite;
            result.Item2 = foundData == null ? default : foundData.spriteScale;

            // hotfix
            bool shouldUsePinataSprite = currencyType.IsMonopolyCurrency() || (!shouldUseDiceSprite && currencyType == CurrencyType.Premium);
            if (shouldUsePinataSprite)
            {
                result.Item1 = pinataFrozenSprite;
                result.Item2 = Vector3.one;
            }

            return result;
        }


        public (Sprite, Vector3) FindFrezeePetSkinSpriteAndScale()
        {
            (Sprite, Vector3) result = (default, default);

            result.Item1 = petFrozenSprite;
            result.Item2 = Vector3.one;

            return result;
        }


        public string FindFrezeeFx(PhysicalLevelObjectData data)
        {
            VisualFxData foundData = Array.Find(visualFxData, e => Array.Exists(e.shapeType, t => t == data.shapeType) &&
                                                                   Array.Exists(e.sizeType, t => t == data.sizeType));

            Log(foundData == null, $"No fxs found for data type = {data.type} shapeType = {data.shapeType} sizeType = {data.sizeType}");
            return foundData == null ? default : foundData.fxKey;
        }


        public string FindCurrencyDestroyFx(CurrencyType currencyType)
        {
            DestroyFxData foundData = Array.Find(destroyFxData, e => e.currencyType == currencyType);

            Log(foundData == null, $"No fxs found for data currencyType = {currencyType}");
            return foundData == null ? default : foundData.fxKey;
        }


        private void Log(bool condition, string message)
        {
            if (condition)
            {
                CustomDebug.Log(message);
            }
        }

        #endregion
    }
}

