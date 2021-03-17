using System;
using Modules.Sound;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Helpers
{
    public class ButtonSound : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Button currentButton = default;

        [SerializeField]
        [Enum(typeof(AudioKeys.Ui))]
        private string onClickSound = default;

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            if (currentButton != null)
            {
                currentButton.onClick.AddListener(PlaySound);
            }
        }


        private void OnDestroy()
        {
            if (currentButton != null)
            {
                currentButton.onClick.RemoveListener(PlaySound);
            }
        }


        private void Reset()
        {
            currentButton = GetComponent<Button>();
        }

        #endregion



        #region Methods

        public void SetupAudioKey(string key)
        {
            onClickSound = key;
        }


        private void PlaySound()
        {
            SoundManager.Instance.PlaySound(onClickSound);
        }

        #endregion
    }
}
