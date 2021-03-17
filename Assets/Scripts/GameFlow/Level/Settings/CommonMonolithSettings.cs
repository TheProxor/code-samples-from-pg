using UnityEngine;
using System;
using UnityEngine.U2D;
using Drawmasters.Monolith;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public enum MonolithFillType
    {
        None        =   0,
        Bricks      =   1,
        Stones      =   2,
        Sausages    =   3,
        Squares     =   4
    }

    [CreateAssetMenu(fileName = "CommonMonolithSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "CommonMonolithSettings")]
    public class CommonMonolithSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class CornerData
        {
            public int angle = default;
            public Sprite innerSprite = default;
            public Sprite outerSprite = default;
        }

        [Serializable]
        private class CornerPrefabData
        {
            public int angle = default;
            public CornerGraphic innerObject = default;
            public CornerGraphic outerObject = default;
        }
        

        [Serializable]
        private class MonolithFillData
        {
            public MonolithFillType fillType = default;

            public Texture2D colorableFillTexture = default;
        }

        #endregion



        #region Fields

        [Header("Colorable textures")]
        [SerializeField] private MonolithFillData[] monolithFillDataList = default;

        [Header("Monolith contur sprite")]
        public Sprite conturMonolithSprite = default;

        [Header("Level target impuls settings")]
        public float mass = default;
        public float damageConversionImpulsMultiplier = default;

        [Header("Sprite shape")]
        [SerializeField] private SpriteShape commonSpriteShape = default;

        [Header("Colorable corners")]
        [SerializeField] private List<CornerData> colorableCorners = default;

        #endregion



        #region Methods

        public SpriteShape CreateSpriteShape() => Instantiate(commonSpriteShape);


        public Sprite GetCornerSprite(CornerFormType formType, int angle)
        {
            Sprite result = default;

            CornerData foundCornerData = colorableCorners.Find(data => Mathf.Abs(data.angle - angle) <= 1);

            if (foundCornerData == null)
            {
                CustomDebug.Log($"No corner sprite for {angle} anle.");

                return result;
            }

            if (formType == CornerFormType.Inner)
            {
                result = foundCornerData.innerSprite;
            }
            else
            {
                result = foundCornerData.outerSprite;
            }

            return result;
        }
        
        
        public Texture2D GetFillTexture(MonolithFillType fillType)
        {
            Texture2D result = default;

            MonolithFillData data = Array.Find(monolithFillDataList, c => c.fillType == fillType);

            if (data != null)
            {
                result = data.colorableFillTexture;
            }
            else
            {
                CustomDebug.Log("Missing colorable fill texture. Type : " + fillType);
            }

            return result;
        }


        public Sprite GetMonolithConturSprite() => conturMonolithSprite;

        #endregion
    }
}
