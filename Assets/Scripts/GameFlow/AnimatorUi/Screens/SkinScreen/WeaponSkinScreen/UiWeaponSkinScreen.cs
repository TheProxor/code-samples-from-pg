using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class UiWeaponSkinScreen : UiSkinScreen
    {
        #region Fields

        [Header("Weapon skin scroll data")]
        [SerializeField] [Required]
        private UiSkinScrollData skinScrollData = default;
        
        #endregion


        
        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.WeaponSkinScreen;


        protected override SkinScreenState InitialScrollState =>
             SkinScreenState.Weapon;


        protected override Dictionary<SkinScreenState, UISkinScrollBehaviour> InitialBehaviours
        {
            get
            {
                var result = new Dictionary<SkinScreenState, UISkinScrollBehaviour>
                {
                    { SkinScreenState.Weapon, new WeaponSkinScroll(skinScrollData) }
                };

                return result;
            }
        }

        #endregion
    }
}
