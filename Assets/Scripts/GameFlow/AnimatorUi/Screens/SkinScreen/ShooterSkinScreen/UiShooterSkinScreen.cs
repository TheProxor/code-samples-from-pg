using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class UiShooterSkinScreen : UiSkinScreen
    {
        #region Fields
        
        [Header("Shooter skin scroll data")]
        [SerializeField] [Required]
        private UiSkinScrollData skinScrollData = default;
        
        #endregion


        
        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.ShooterSkinScreen;


        protected override SkinScreenState InitialScrollState =>
             SkinScreenState.Shooter;


        protected override Dictionary<SkinScreenState, UISkinScrollBehaviour> InitialBehaviours
        {
            get
            {
                var result = new Dictionary<SkinScreenState, UISkinScrollBehaviour>
                {
                    { SkinScreenState.Shooter, new ShooterSkinScroll(skinScrollData) }
                };

                return result;
            }
        }

        #endregion
    }
}
