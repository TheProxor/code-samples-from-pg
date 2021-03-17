using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "CommonBackgroundsSettings",
                 menuName = NamingUtility.MenuItems.IngameSettings + "CommonBackgroundsSettings")]
    public class CommonBackgroundsSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class BackgroundData
        {
            public int backgroundIndex = default;
            
            public Sprite sprite = default;
            public Sprite uiMainMenuProgressSprite = default;
        }

        #endregion



        #region Fields
        
        [SerializeField] private Sprite whiteSprite = default;

        [SerializeField] private List<BackgroundData> backgroundsData = default;

        #endregion



        #region Properties

        public string[] BackgroundNames => backgroundsData.Select(b => b.backgroundIndex.ToString()).ToArray();


        public Sprite WhiteSprite => whiteSprite;


        public Sprite RandomUiMainMenuProgressSprites(Sprite exceptedSprite)
            => backgroundsData.Select(b => b.uiMainMenuProgressSprite).Where(sp => sp != exceptedSprite).ToList().RandomObject();

        #endregion



        #region Methods
        
        public Sprite FindBackgroundSprite(int backgroundIndex)
        {
            BackgroundData data = backgroundsData.Find(b => b.backgroundIndex == backgroundIndex);

            return data.sprite;
        }


        public Sprite FindUiMainMenuProgressSprite(int backgroundIndex)
        {
            BackgroundData data = backgroundsData.Find(b => b.backgroundIndex == backgroundIndex);

            return data.uiMainMenuProgressSprite;
        }

        #endregion
    }
}
