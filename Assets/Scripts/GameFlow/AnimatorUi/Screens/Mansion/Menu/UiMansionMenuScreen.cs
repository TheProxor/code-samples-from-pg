using Drawmasters.ServiceUtil;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class UiMansionMenuScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] private Localize bodyText = default;
        [SerializeField] private Button[] closeButtons = default;

        #endregion 



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.MansionMenu;

        #endregion



        #region Methods

        public override void Show()
        {
            base.Show();

            bodyText.SetStringParams(GameServices.Instance.AbTestService.CommonData.minLevelForMansion);
        }


        public override void InitializeButtons()
        {
            foreach (var closeButton in closeButtons)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }


        public override void DeinitializeButtons()
        {
            foreach (var closeButton in closeButtons)
            {
                closeButton.onClick.RemoveListener(Hide);
            }
        }

        #endregion
    }
}
