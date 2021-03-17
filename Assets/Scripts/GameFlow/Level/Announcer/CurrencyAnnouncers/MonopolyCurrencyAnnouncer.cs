using TMPro;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;


namespace Drawmasters.Announcer
{
    public class MonopolyCurrencyAnnouncer : CommonAnnouncer
    {
        #region Fields

        [SerializeField] private Image iconImage = default;
        [SerializeField] private TMP_Text text = default;

        #endregion


        #region Methods

#warning It's not a target text, it can be as localization key as well as text. Fix it. To Dmitry S
        public void SetupValues(Sprite sprite, string targetText)
        {
            iconImage.sprite = sprite;
            iconImage.SetNativeSize();

            if (text.TryGetComponent(out Localize localize))
            {
                localize.SetTerm(targetText);
            }
            else
            {
                text.SetText(targetText);
            }
        }

        #endregion
    }
}
