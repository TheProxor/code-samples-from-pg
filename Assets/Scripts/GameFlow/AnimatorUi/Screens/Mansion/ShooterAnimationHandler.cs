using Drawmasters.Utils;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui.Mansion
{
    public class ShooterAnimationHandler : MonoBehaviour
    {
        #region Constants

        private const string ShowTrigger = "Show";

        private const string Shown = "Shown";

        private const string HideTrigger = "Hide";

        private const string Hidden = "Hidden";

        #endregion



        #region Fields

        [SerializeField] private Animator previewAnimator = default;
        [SerializeField] private Button previewButton = default;

        [SerializeField] private SkeletonGraphic characterAnimation = default;

        [SpineAnimation(dataField = "characterAnimation")]
        [SerializeField] private string showAnimation = default;

        [SpineAnimation(dataField = "characterAnimation")]
        [SerializeField] private string idleAnimation = default;

        [SerializeField] private bool isCharacterSeparateAnimation = default;

        [SerializeField] private SkeletonGraphic additionalAnimation = default;

        [SpineAnimation(dataField = "additionalAnimation")]
        [SerializeField] private string additionalAnimationName = default;

        #endregion



        #region Public methods

        public void ShowCharacterImmediately()
        {
            if (isCharacterSeparateAnimation)
            {
                characterAnimation.enabled = true;
            }

            previewButton.enabled = false;

            previewAnimator.SetTrigger(Hidden);

            if (!string.IsNullOrEmpty(idleAnimation))
            {
                SpineUtility.SafeSetAnimation(characterAnimation, idleAnimation, 0, true);
            }

            if (additionalAnimation != null && !string.IsNullOrEmpty(additionalAnimationName))
            {
                SpineUtility.SafeSetAnimation(additionalAnimation, additionalAnimationName, 0, true);
            }
        }


        public void ShowCharacter()
        {
            if (isCharacterSeparateAnimation)
            {
                characterAnimation.enabled = true;
            }
            previewButton.enabled = false;

            previewAnimator.SetTrigger(HideTrigger);

            SpineUtility.SafeSetAnimation(characterAnimation, showAnimation, 0, true);

            if (additionalAnimation != null && !string.IsNullOrEmpty(additionalAnimationName))
            {
                SpineUtility.SafeSetAnimation(additionalAnimation, additionalAnimationName, 0, true);
            }
        }


        public void ShowPreviewImmediately()
        {
            if (isCharacterSeparateAnimation)
            {
                characterAnimation.enabled = false;
            }
            previewButton.enabled = true;

            previewAnimator.SetTrigger(Shown);
        }


        public void ShowPreview()
        {
            if (isCharacterSeparateAnimation)
            {
                characterAnimation.enabled = false;
            }

            previewButton.enabled = true;

            previewAnimator.SetTrigger(ShowTrigger);
        }


        public void HidePreview()
        {
            if (isCharacterSeparateAnimation)
            {
                characterAnimation.enabled = false;
            }

            previewButton.enabled = false;

            previewAnimator.SetTrigger(HideTrigger);
        }


        public void HidePreviewImmediately()
        {
            previewButton.enabled = false;

            previewAnimator.SetTrigger(Hidden);

            CommonUtility.SetObjectActive(previewButton.gameObject, false);
        }


        public void HideAllImmediately()
        {
            previewAnimator.SetTrigger(Hidden);

            previewButton.enabled = false;

            if (isCharacterSeparateAnimation)
            {
                characterAnimation.enabled = false;
            }
        }

        #endregion
    }
}
