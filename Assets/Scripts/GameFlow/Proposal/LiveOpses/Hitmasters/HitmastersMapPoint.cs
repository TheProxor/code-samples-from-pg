using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils.Ui;
using Modules.General;
using Modules.Sound;


namespace Drawmasters.Proposal
{
    public class HitmastersMapPoint : MonoBehaviour
    {
        #region Nested types

        public enum MapPointType
        {
            Level       = 0,
            Suitcase    = 1,
            Shop        = 2,
            Gems        = 3,
            Forcemeter  = 4,
            Boss        = 5
        }

        #endregion



        #region Fields

        [SerializeField] protected Image shadowIcon = default;
        [SerializeField] private TMP_Text textInfo = default;
        [SerializeField] private bool forestPosition = default;

        [SerializeField] private BlendImage icon = default;

        [Header("Effects")] [SerializeField] private Transform idleFxRoot = default;

        private HitmastersProposeController controller;
        private int index;
        private MapPointType type = MapPointType.Level;
        private EffectHandler idleActiveEffectHandler;
        private EffectHandler idleEffectHandler;

        private VectorAnimation activePointBounceAnimation;

        #endregion



        #region Methods

        public void Initialize(int _index, MapPointType pointType)
        {
            index = _index;

            type = pointType;

            controller = GameServices.Instance.ProposalService.HitmastersProposeController;
            activePointBounceAnimation = new VectorAnimation();
            activePointBounceAnimation.SetupData(controller.VisualSettings.activePointBounceAnimation);

            Sprite currentSprite;

            if (controller.LiveOpsLevelCounter < index)
            {
                currentSprite = controller.VisualSettings.FindMapPointLockedIcon(type);
                PlayIdleFx();
            }
            else if (controller.LiveOpsLevelCounter == index)
            {
                currentSprite = controller.VisualSettings.FindMapPointActiveIcon(type);
                SetTextColor(false, true);

                PlayActiveIdleFx();
                PlayIdleFx();
            }
            else
            {
                currentSprite = controller.VisualSettings.FindMapPointDisableIcon(type);
                SetTextColor(true, true);
            }

            SetupText();
            icon.sprite = currentSprite;
            icon.SetNativeSize();

            shadowIcon.sprite = controller.VisualSettings.FindMapPointShadowIcon(type);
            shadowIcon.color = forestPosition
                ? controller.VisualSettings.mapPointShadowForestColor
                : controller.VisualSettings.mapPointShadowCanyonColor;
            shadowIcon.SetNativeSize();

            icon.CreateTextureComponent(controller.VisualSettings.materialBlendAnimation, 
                currentSprite.texture, 
                null);
            
            icon.BlendTextureComponent.Initialize();
        }



        public void Deinitialize()
        {
            icon.BlendTextureComponent.Deinitialize();

            StopActiveIdleFx();
            StopIdleFx();

            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public void PlayCompletedAnimation()
        {
            DOTween.Complete(this);

            Sprite activeSprite = controller.VisualSettings.FindMapPointActiveIcon(type);
            Sprite completedSprite = controller.VisualSettings.FindMapPointDisableIcon(type);

            if (activeSprite != null && 
                completedSprite != null)
            {
                icon.sprite = activeSprite;
                icon.SetNativeSize();

                icon.BlendTextureComponent.SetupTextures(activeSprite.texture, completedSprite.texture);
                icon.BlendTextureComponent.BlendToSecond();
            }
            else
            {
                CustomDebug.Log($"ActiveSprite or completedSprite is NULL in {this}");
            }
         
            SetTextColor(true, false);
        }


        public void PlayUnlockedAnimation()
        {
            DOTween.Complete(this);

            Sprite activeSprite = controller.VisualSettings.FindMapPointActiveIcon(type);
            Sprite completedSprite = controller.VisualSettings.FindMapPointLockedIcon(type);

            if (activeSprite != null && 
                completedSprite != null)
            {
                icon.sprite = completedSprite;
                icon.SetNativeSize();

                icon.BlendTextureComponent.SetupTextures(completedSprite.texture, activeSprite.texture);
            }
            else
            {
                CustomDebug.Log($"ActiveSprite or completedSprite is NULL in {this}");
            }

            textInfo.text = string.Empty;
            SetTextColor(false, true);
            icon.BlendTextureComponent.BlendToSecond(false);
            StopActiveIdleFx();

            Scheduler.Instance.CallMethodWithDelay(this, 
                PlayAction, 
                controller.VisualSettings.playUnlockedAnimationDelay);

            void PlayAction()
            {
                activePointBounceAnimation.delay = controller.VisualSettings.activePointBounceAnimation.delay;
                activePointBounceAnimation.Play(value => transform.localScale = value, this, () =>
                {
                    SetupText();

                    string unlockFxKey = controller.VisualSettings.FindUnlockFxKey(type);
                    
                    var unlockFxHandler = EffectManager.Instance.PlaySystemOnce(unlockFxKey, 
                        idleFxRoot.position, 
                        idleFxRoot.rotation, 
                        idleFxRoot);

                    if (unlockFxHandler != null)
                    {
                        unlockFxHandler.transform.localScale = controller.VisualSettings.FindUnlockFxKeyScale(type);
                    }

                    PlayActiveIdleFx();
                    
                    activePointBounceAnimation.delay = 0.0f;
                    activePointBounceAnimation.Play(value => transform.localScale = value, this, null, true);
                });

                SoundManager.Instance.PlayOneShot(AudioKeys.Ui.LIVEOPS_MAP_UNLOCK);
            }
        }


        private void PlayActiveIdleFx()
        {
            string fxKey = controller.VisualSettings.FindMapPointActiveFxKey(type);
            if (string.IsNullOrEmpty(fxKey))
            {
                return;
            }
            
            idleActiveEffectHandler = EffectManager.Instance.CreateSystem(fxKey, 
                true, 
                default, 
                default, 
                idleFxRoot, 
                TransformMode.Local, 
                true);

            idleActiveEffectHandler.transform.localScale = Vector3.one;
            idleActiveEffectHandler.Play();
        }


        private void PlayIdleFx()
        {
            string idleLockedFxKey = controller.VisualSettings.FindMapPointFxKey(type);
            if (string.IsNullOrEmpty(idleLockedFxKey))
            {
                return;
            }
            
            idleEffectHandler = EffectManager.Instance.CreateSystem(idleLockedFxKey, 
                true, 
                default, 
                default, 
                idleFxRoot, 
                TransformMode.Local, 
                true);

            idleEffectHandler.transform.localScale = Vector3.one;
            idleEffectHandler.Play();
        }


        private void StopActiveIdleFx() =>
            EffectManager.Instance.ReturnHandlerToPool(idleActiveEffectHandler);


        private void StopIdleFx() =>
            EffectManager.Instance.ReturnHandlerToPool(idleEffectHandler);


        private void SetTextColor(bool isDisabled, bool isImmediately)
        {
            ColorAnimation materialTextAnimation = controller.VisualSettings.materialTextAnimation;

            if (isImmediately)
            {
                textInfo.color = isDisabled ? 
                    materialTextAnimation.endValue : 
                    materialTextAnimation.beginValue;
            }
            else
            {
                textInfo.color = !isDisabled ? 
                    materialTextAnimation.endValue : 
                    materialTextAnimation.beginValue;
                
                materialTextAnimation.Play(value =>
                {
                    textInfo.color = value;
                }, 
                this, 
                null, 
                !isDisabled);
            }
        }


        private void SetupText()
        {
            bool shouldShowText = controller.LiveOpsLevelCounter >= index && 
                                  type != MapPointType.Boss;
            
            textInfo.text = shouldShowText ? 
                (index + 1).ToString("D") : 
                string.Empty;
        }

        #endregion
    }
}
