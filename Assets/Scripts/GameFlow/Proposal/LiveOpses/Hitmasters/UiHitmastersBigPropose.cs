using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Drawmasters.Ui
{
    public class UiHitmastersBigPropose : UiHitmastersBasePropose
    {
        #region Fields
        
        [SerializeField] private TMP_Text descriptionText = default;

        #endregion


        
        #region Methods
        
        protected override void OnShouldRefreshVisual()
        {
            base.OnShouldRefreshVisual();

            GameMode currentMode = CurrentGameMode;
            
            descriptionText.text = controller.VisualSettings.FindUiMenuText(currentMode);

            string fxKey = controller.VisualSettings.FindMenuIdleFx(currentMode);
            idleEffect.SetFxKey(fxKey);

            idleEffect.StopEffect();
            idleEffect.CreateAndPlayEffect();
        }

        #endregion
    }
}
