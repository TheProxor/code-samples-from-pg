using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Levels.Order
{
    [CreateAssetMenu(fileName = "ColorProfilesSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "ColorProfilesSettings")]
    public class ColorProfilesSettings : ScriptableObject
    {
        #region Helpers

        [Serializable]
        private class ProfileData
        {
            public int colorProfileIndex = default;
            public ColorProfile profile = default;
        }

        #endregion
        


        #region Fields

        [SerializeField] private List<ProfileData> profiles = default;


        #endregion



        #region Properties

        public int RandomProfileIndex => profiles.Select(i => i.colorProfileIndex).ToList().RandomObject();

        #endregion



        #region Methods

        public ColorProfile GetProfile(int index) => profiles.Find(p => p.colorProfileIndex == index).profile;        
            
        #endregion
    }
}
