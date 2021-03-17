using System;
using System.Collections.Generic;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ShootersInputLevelController : SwitchableLevelController
    {
        #region Fields

        public static event Action<Shooter, Vector2> OnStartDraw;
        public static event Action<Shooter, Vector2> OnDraw;
        public static event Action<Shooter, bool, Vector2> OnDrawFinish;

        private readonly Dictionary<Shooter, ShooterInput> shooters = new Dictionary<Shooter, ShooterInput>();

        private Vector2 previousPoint;

        private CaptureModule<ShooterTapZone> shooterCaptureModule;
        private CaptureModule<LevelTargetLink> targetsCaptureModule; 

        private readonly List<LevelTarget> markedLevelTargets = new List<LevelTarget>();

        private LevelTargetController targetController;
        private readonly LevelPathController pathController;
        private readonly BonusLevelShooterInputHelper bonusLevelInput;


        #endregion



        #region Abstract implementation

        protected override bool IsControllerEnabled
        {
            get
            {
                bool result = false;

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;

                if (context != null)
                {
                    result = context.Mode.IsDrawingMode();
                }

                return result;
            }
        }

        #endregion


        #region Ctor

        public ShootersInputLevelController(LevelPathController _pathController, BonusLevelController _bonusLevelController)
        {
            pathController = _pathController;
            bonusLevelInput = new BonusLevelShooterInputHelper(_bonusLevelController);
        }

        #endregion


        #region Methods

        public override void CustomInitialize()
        {
            targetController = GameServices.Instance.LevelControllerService.Target;

            int levelTargetsMask = LayerMask.GetMask(PhysicsLayers.Enemy);
            shooterCaptureModule = new CaptureModule<ShooterTapZone>(IngameCamera.Instance.Camera, levelTargetsMask);
            shooterCaptureModule.Initialize();
            shooterCaptureModule.OnCapture += ShooterCaptureModule_OnShooterCaptures;

            targetsCaptureModule = new CaptureModule<LevelTargetLink>(IngameCamera.Instance.Camera, levelTargetsMask);
            targetsCaptureModule.Initialize();
            targetsCaptureModule.OnCapture += LevelTargetCaptureModule_OnShooterCaptures;
            
            bonusLevelInput.Initialize();

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            ShooterInput.OnShouldForbidInput += ShooterInput_OnShouldForbidInput;
        }


        public override void CustomDeinitialize()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            ShooterInput.OnShouldForbidInput -= ShooterInput_OnShouldForbidInput;

            markedLevelTargets.Clear();

            foreach (var data in shooters)
            {
                data.Value.Deinitialize();
            }

            shooters.Clear();

            shooterCaptureModule.Deinitialize();
            shooterCaptureModule.OnCapture -= ShooterCaptureModule_OnShooterCaptures;

            targetsCaptureModule.OnCapture -= LevelTargetCaptureModule_OnShooterCaptures;
            targetsCaptureModule.Deinitialize();
            
            bonusLevelInput.Deinitialize();

            TouchManager.OnMove -= InputTouch_OnMove;
            TouchManager.OnEnded -= InputTouch_OnEnded; 
        }


        public void Add(Shooter shooter)
        {
            if (!IsControllerEnabled)
            {
                return;
            }

            ShooterInput shooterInput = new ShooterInput(shooter.transform.position);
            shooterInput.Initialize();

            shooter.SetInputModule(shooterInput);
            shooters.Add(shooter, shooterInput);
        }


        private bool TryPerfomActionForInput(Shooter shooter, Action<ShooterInput> action)
        {
            if (shooters.TryGetValue(shooter, out ShooterInput input))
            {
                action?.Invoke(input);
                return true;
            }

            return false;
        }

        #endregion



        #region Events handlers

        private void LevelTargetCaptureModule_OnShooterCaptures(LevelTargetLink captured, Vector2 touchPosition)
        {
            if (shooterCaptureModule.IsCaptured &&
                captured.LevelTarget.Type.IsEnemy() &&
                ColorTypesSolutions.CanCaptureEnemy(captured.LevelTarget.ColorType, shooterCaptureModule.Current.Shooter.ColorType, captured.LevelTarget.Type))
            {
                markedLevelTargets.AddExclusive(captured.LevelTarget);

                bool isAllTargetMarked = markedLevelTargets.Count >= targetController.GetEnemiesCount(captured.LevelTarget.ColorType);

                if (isAllTargetMarked)
                {
                    InputTouch_OnEnded(true, touchPosition);
                }
            }

            targetsCaptureModule.ResetCapture();
        }


        private void ShooterCaptureModule_OnShooterCaptures(ShooterTapZone captured, Vector2 touchPosition)
        {
            if (shooterCaptureModule.IsCaptured)
            {
                if (pathController.IsControllerEnabled && !pathController.CanShooterDraw(captured.Shooter))
                {
                    shooterCaptureModule.ResetCapture();
                   
                    return;
                }

                if (!bonusLevelInput.IsAimingAvailable)
                {
                    shooterCaptureModule.ResetCapture();
                    return;
                }
                
                markedLevelTargets.Clear();

                previousPoint = touchPosition;

                TryPerfomActionForInput(captured.Shooter, (input) => input.InvokeOnStartDraw(touchPosition));

                TouchManager.OnMove += InputTouch_OnMove;
                TouchManager.OnEnded += InputTouch_OnEnded;

                OnStartDraw?.Invoke(captured.Shooter, touchPosition);
            }
        }


        private void InputTouch_OnMove(Vector2 touchPosition)
        {
            if (shooterCaptureModule.IsCaptured &&
                Vector2.Distance(previousPoint, touchPosition) > IngameData.Settings.shooter.input.minDistanceBetweenPoints)
            {
                TryPerfomActionForInput(shooterCaptureModule.Current.Shooter, (input) => input.InvokeOnDraw(touchPosition));
                previousPoint = touchPosition;
                
                OnDraw?.Invoke(shooterCaptureModule.Current.Shooter, touchPosition);

                if (pathController.IsControllerEnabled && pathController.IsPathDistanceLimit)
                {
                    InputTouch_OnEnded(true, touchPosition);
                }
            }
        }


        private void InputTouch_OnEnded(bool success, Vector2 touchPosition)
        {
            if (shooterCaptureModule.IsCaptured)
            {
                TryPerfomActionForInput(shooterCaptureModule.Current.Shooter, (input) => input.InvokeOnFinishDraw(success, touchPosition));
                OnDrawFinish?.Invoke(shooterCaptureModule.Current.Shooter, success, touchPosition);

                shooterCaptureModule.ResetCapture();
            }

            TouchManager.OnMove -= InputTouch_OnMove;
            TouchManager.OnEnded -= InputTouch_OnEnded;
        }


        // TODO: temp. just for test
        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.AllTargetsHitted)
            {
                Deinitialize();
            }
        }


        private void ShooterInput_OnShouldForbidInput()
        {
            shooterCaptureModule.OnCapture -= ShooterCaptureModule_OnShooterCaptures;
            TouchManager.OnMove -= InputTouch_OnMove;
            TouchManager.OnEnded -= InputTouch_OnEnded;
        }


        #endregion
    }
}