using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using Drawmasters.Proposal;
using Spine.Unity;
using UnityEngine;
using Modules.Sound;
using System.Collections.Generic;
using DG.Tweening;
using System;
using Modules.General;

namespace Drawmasters.Ui
{
    public class UiHitmastersPopupScreen : UiLiveOpsPreviewScreen
    {
        #region Fields

        [SerializeField] private IdleEffect idleEffect = default;
        [SerializeField] private SkeletonGraphic shooterSkinAnimation = default;

        private readonly List<AnimationEffectPlayerHandler> animationEffectPlayerHandlers = new List<AnimationEffectPlayerHandler>();
        private AnimationActionHandler animationActionHandler;

        private RectTransform noriProjectileRectTransform;
        private HitmastersProposeController controller;

        private object noriShotGuid;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.HitmastersPreview;

        #endregion



        #region Methods

        public override void Show()
        {
            noriShotGuid = Guid.NewGuid();
            controller = GameServices.Instance.ProposalService.HitmastersProposeController;

            base.Show();

            //TOODL
            animationActionHandler = new AnimationActionHandler(shooterSkinAnimation, controller.VisualSettings.noriShotEvent);
            animationActionHandler.Initialize();
            animationActionHandler.OnEventHappened.AddListener(PerformNoriShot);

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.LIVEOPS_POPUP_APPEAR);
        }


        public override void Deinitialize()
        {
            if (animationActionHandler != null)
            {
                animationActionHandler.OnEventHappened.RemoveListener(PerformNoriShot);
                animationActionHandler.Deinitialize();
            }

            DestroyNoriProjectile();

            idleEffect.StopEffect();

            ClearFxHandlers();
            
            DOTween.Kill(noriShotGuid);
            Scheduler.Instance.UnscheduleAllMethodForTarget(noriShotGuid);

            base.Deinitialize();
        }


        protected override LiveOpsProposeController GetController() =>
            GameServices.Instance.ProposalService.HitmastersProposeController;


        protected override void OnShouldShowLiveOpsScreen()
        {
            UiScreenManager.Instance.ShowScreen(ScreenType.HitmastersMap, isForceHideIfExist: true);
        }


        protected override void RefreshVisual()
        {
            base.RefreshVisual();

            (ShooterSkinType type, SkeletonDataAsset asset, string startAnimation, string loopAnimation) =
                controller.VisualSettings.FindGameModeSkinAnimationData(controller.LiveOpsGameMode);

            idleEffect.StopEffect();

            string idleFxKey = controller.VisualSettings.FindPopupIdleFxKey(controller.LiveOpsGameMode);
            idleEffect.SetFxKey(idleFxKey);
            idleEffect.CreateAndPlayEffect();

            SetupShooterSkin(type, asset, startAnimation, loopAnimation);

            ClearFxHandlers();
            AddFxHandlers();
        }


        private void SetupShooterSkin(ShooterSkinType type, SkeletonDataAsset asset, string startAnimation, string loopAnimation)
        {
            shooterSkinAnimation.skeletonDataAsset = asset;
            shooterSkinAnimation.initialSkinName = string.Empty;
            shooterSkinAnimation.Initialize(true);

            SpineUtility.SetShooterSkin(type, shooterSkinAnimation);

            SpineUtility.SafeSetAnimation(shooterSkinAnimation, startAnimation);
            SpineUtility.SafeAddAnimation(shooterSkinAnimation, loopAnimation, 0, true);
        }


        private void AddFxHandlers()
        {
            HitmastersVisualSettings.EventFxData[] eventFxDatas = controller.VisualSettings.FindPopupEventFxData(controller.LiveOpsGameMode);

            foreach (var data in eventFxDatas)
            {
                AnimationEffectPlayerHandler handler = new AnimationEffectPlayerHandler(shooterSkinAnimation, data.animEvent, data.bone, data.fxKey);
                handler.SetAttachToRoot(data.shouldAttachToRoot);
                handler.SetStopFxEvent(data.animStopEvent);
                animationEffectPlayerHandlers.Add(handler);
            }

            foreach (var handler in animationEffectPlayerHandlers)
            {
                handler.Initialize();
            }
        }


        private void ClearFxHandlers()
        {
            foreach (var handler in animationEffectPlayerHandlers)
            {
                handler.Deinitialize();
            }

            animationEffectPlayerHandlers.Clear();
        }


        private void PerformNoriShot()
        {
            DOTween.Kill(noriShotGuid);
            Scheduler.Instance.UnscheduleAllMethodForTarget(noriShotGuid);

            Vector2[] shotDirections = controller.VisualSettings.noriProjectileShotDirections;

            Vector3 startBonePosition = SpineUtility.BoneToWorldPosition(controller.VisualSettings.noriShotBone, shooterSkinAnimation, mainCanvas);

            DestroyNoriProjectile();
            noriProjectileRectTransform = Content.Management.Create(controller.VisualSettings.noriProjectilePrefab, mainCanvas.transform);
            noriProjectileRectTransform.localScale = Vector3.one;
            noriProjectileRectTransform.position = startBonePosition;

            Rect mainCanvasRect = (mainCanvas.transform as RectTransform).rect;
            Vector3 lastCalculatedEndValue = noriProjectileRectTransform.anchoredPosition3D;
            float additionalDelay = default;

            for (int i = 0; i < shotDirections.Length; i++)
            {
                Vector3 direction = shotDirections[i];
                Vector3 beginValue = lastCalculatedEndValue;
                Vector3 endValue = CalculateScreenBorderPosition(beginValue, direction);
                lastCalculatedEndValue = endValue;

                VectorAnimation vectorAnimation = new VectorAnimation();
                vectorAnimation.SetupData(controller.VisualSettings.noriProjectileShotAnimation);

                float duration = Vector3.Distance(beginValue, endValue) / controller.VisualSettings.noriProjectileSpeed;
                vectorAnimation.SetupDuration(duration);
                vectorAnimation.SetupBeginValue(beginValue);
                vectorAnimation.SetupEndValue(endValue);

                Action callback = i == shotDirections.Length - 1 ? 
                    DestroyNoriProjectile : (Action)null;
                
                callback += () => EffectManager.Instance.PlaySystemOnce(EffectKeys.FxWeaponStickCollisionGui, 
                    mainCanvas.transform.TransformPoint(endValue), 
                    Quaternion.identity, 
                    mainCanvas.transform);

                Scheduler.Instance.CallMethodWithDelay(noriShotGuid, () =>
                {
                    float angle = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg + 90.0f;
                    noriProjectileRectTransform.localEulerAngles = Vector3.forward * angle;

                    vectorAnimation.Play(value => 
                        noriProjectileRectTransform.anchoredPosition = value, noriShotGuid, callback);
                    
                  }, additionalDelay);

                additionalDelay += vectorAnimation.Time;

                Vector3 CalculateScreenBorderPosition(Vector3 startPoint, Vector3 dir)
                {
                    // To avoid self point intersection
                    const float offsetEpsilon = 0.05f;

                    Vector3 startPointEpsilon = startPoint + dir.normalized * offsetEpsilon;

                    bool isIntersect = CommonUtility.LineRectIntersectionByDirection(startPointEpsilon, 
                        dir, 
                        mainCanvasRect, 
                        out Vector3 intersection);

                    if (!isIntersect)
                    {
                        CustomDebug.Log("No intersection found!");
                        return default;
                    }

                    return intersection;
                }
            }
        }


        private void DestroyNoriProjectile()
        {
            if (noriProjectileRectTransform != null)
            {
                Content.Management.DestroyObject(noriProjectileRectTransform.gameObject);
                noriProjectileRectTransform = null;
            }
        }

        #endregion
    }
}
