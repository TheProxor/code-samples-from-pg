using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;


namespace Drawmasters.Ui
{
    public class UiPetSkinScreen : UiSkinScreen
    {
        #region Fields

        [Header("Pet skin scroll data")]
        [SerializeField] [Required]
        private UiSkinScrollData skinScrollData = default;
        
        #endregion


        
        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.PetSkinScreen;


        protected override SkinScreenState InitialScrollState =>
             SkinScreenState.Pet;


        protected override Dictionary<SkinScreenState, UISkinScrollBehaviour> InitialBehaviours
        {
            get
            {
                var result = new Dictionary<SkinScreenState, UISkinScrollBehaviour>
                {
                    { SkinScreenState.Pet, new PetSkinScroll(skinScrollData) }
                };

                return result;
            }
        }

        #endregion
    }
}
