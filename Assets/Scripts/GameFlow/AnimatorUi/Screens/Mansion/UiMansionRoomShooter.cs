using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui.Mansion
{
    public class UiMansionRoomShooter : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Image previewIconImage = default;

        [SerializeField] private ShooterSkinType shooterSkinType = default;

        [SerializeField] private ShooterAnimationHandler animationHandler = default;

        [SerializeField] private AnimationEffectPlayer[] animationEffectsPlayer = default;

        [SerializeField] private Transform appearFxRoot = default;

        // TODO dirty
        bool wasCompleted;

        private UiMansionSwipe swipe;

        #endregion



        #region Properties

        public bool IsBought =>
            GameServices.Instance.ShopService.ShooterSkins.IsBought(shooterSkinType);

        #endregion



        #region Methods

        public void Initialize(bool isCompleted, UiMansionSwipe _swipe)
        {
            if (previewIconImage != null)
            {
                Sprite previewSprite = IngameData.Settings.mansionRewardPackSettings.GetPreviewSprite(shooterSkinType);
                if (previewSprite != null)
                {
                    previewIconImage.sprite = previewSprite;
                    previewIconImage.SetNativeSize();
                }
            }
            else
            {
                // TODO temporary log
                CustomDebug.Log("Missing preview icon image reference in: " + gameObject.name, this);
            }

            wasCompleted = isCompleted;

            swipe = _swipe;
            swipe.OnSwipeBegin += Swipe_OnSwipeBegin;
            swipe.OnSwipeEnd += Swipe_OnSwipeEnd;

            foreach (var animEffect in animationEffectsPlayer)
            {
                animEffect.Initialize();
            }
        }


        public void Deinitialize()
        {
            foreach (var animEffect in animationEffectsPlayer)
            {
                animEffect.Deinitialize();
            }

            swipe.OnSwipeBegin -= Swipe_OnSwipeBegin;
            swipe.OnSwipeEnd -= Swipe_OnSwipeEnd;
        }

        public void Refresh(bool isCompleted)
        {
            if (isCompleted)
            {
                if (IsBought)
                {
                    if (!wasCompleted)
                    {
                        animationHandler.ShowCharacter();
                        animationHandler.HidePreviewImmediately();

                        wasCompleted = true;
                        EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUICottageCharAppear, parent: appearFxRoot, transformMode: TransformMode.Local);
                    }
                    else
                    {
                        animationHandler.ShowCharacterImmediately();
                    }
                }
                else
                {
                    if (!wasCompleted)
                    {
                        animationHandler.ShowPreview();
                        wasCompleted = true;
                    }
                    else
                    {
                        animationHandler.ShowPreviewImmediately();
                    }
                }
            }
            else
            {
                animationHandler.HideAllImmediately();
            }
        }

        #endregion



        #region Events handlers

        private void Swipe_OnSwipeEnd()
        {
            if (!IsBought && wasCompleted)
            {
                animationHandler.ShowPreview();
            }
        }

        private void Swipe_OnSwipeBegin()
        {
            if (!IsBought && wasCompleted)
            {
                animationHandler.HidePreview();
            }
        }

        #endregion
    }
}
