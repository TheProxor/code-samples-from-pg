using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Helpers
{
    public class ButtonSoundInspectorSetter : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Enum(typeof(AudioKeys.Ui))]
        private string key = default;

        #endregion



        #region Methods

        [Sirenix.OdinInspector.Button]
        private void SetupSoundKey()
        {
            var buttons = gameObject.GetComponentsInChildren<Button>(true);

            foreach (var button in buttons)
            {
                if (button.GetComponent<ButtonSound>() == null)
                {
                    button.gameObject.AddComponent<ButtonSound>();
                }

                button.GetComponent<ButtonSound>().SetupAudioKey(key);
            }
        }

        #endregion
    }
}
