using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui.NoInternet
{
    public class UiNoInternetScreen : AnimatorScreen
    {
        [SerializeField] private Button retryButton = default;
        
        public override ScreenType ScreenType => ScreenType.NoInternet;
        
        public override void InitializeButtons()
        {
            retryButton.onClick.AddListener(RetryButton_OnClick);
        }
        
        
        public override void DeinitializeButtons()
        {
            retryButton.onClick.RemoveListener(RetryButton_OnClick);
        }


        private void RetryButton_OnClick()
        {
            Hide();
        }
    }
}