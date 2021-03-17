using System;
using Modules.Sound;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class FromLevelToLevel
    {
        #region Methods

        public static void PlayTransition(Action onShowed = null,
                                          Action onHided = null)
        {
            Show(() =>
            {
                onShowed?.Invoke();

                Hide(onHided);
            });

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SWAP_SCREENS);
        }


        public static void Show(Action onShowed)
        {
            UiCamera.Instance.ScaleIn.Run(() => onShowed?.Invoke());
        }


        public static void Hide(Action onHided)
        {
            UiCamera.Instance.ScaleOut.Run(() => onHided?.Invoke());
        }

        #endregion
    }
}
