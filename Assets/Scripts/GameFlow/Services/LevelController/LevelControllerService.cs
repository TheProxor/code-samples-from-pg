using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Levels;
using Drawmasters.Levels.Inerfaces;
using Drawmasters.Pets;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.ServiceUtil
{
    public class LevelControllerService : ILevelControllerService
    {
        #region Fields

        private readonly List<ILevelController> controllers;

        private readonly List<IInitialStateReturn> stateReturnableControllers;
        private readonly List<ILevelStateChangeReporter> levelStateReporters;

        #endregion
         

        #region ILevelStateChangeReporter

        public event Action<LevelState> OnLevelStateChanged;

        #endregion



        #region ILevelControllerService

        public LevelProjectileController Projectile { get; private set; }

        public LevelTargetController Target { get; private set; }

        public LevelPhysicalObjectsController PhysicalObjects { get; private set; }

        public LevelAnnouncersController Announcer { get; private set; }
        
        public LevelSkullAnnouncersController SkullAnnouncer { get; private set; }

        public LevelMotionWin MotionWin { get; private set; }

        public LevelStageController Stage { get; private set; }

        public LevelExtraCurrencyController ExtraCurrency { get; private set; }

        public ShootersInputLevelController ShootersInput { get; private set; }

        public LevelCurrencyCollectController CurrencyCollector { get; private set; }

        public BonusLevelPetCollectController LevelPetCollector { get; private set; }

        public LevelSkullCollectController SkullCollector { get; private set; }
        
        public LevelSoulTrailController SoulTrailController { get; private set; }

        public LevelSoundProjectileController ProjectilesSound { get; private set; }
        
        public LevelPathController Path { get; }
        
        public BonusLevelController BonusLevelController { get; }

        public DrawModeLevelController UsualDrawController { get; }

        public DrawModeLevelController BossDrawController { get; }

        public DrawModeLevelController BonusDrawController { get; }

        public ShootersHitmastersInputLevelController LineInput { get; }

        public LevelCurrencyAnnouncersController LevelCurrencyAnnouncers { get; }

        public PetsInputLevelController PetsInputLevelController { get; }

        public CurrencyObjectsLevelController CurrencyObjectsLevelController { get; }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            foreach(var i in controllers)
            {
                i.Initialize();
            }

            foreach(var i in levelStateReporters)
            {
                i.OnLevelStateChanged += Controller_OnLevelStateChanged;
            }
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            foreach(var i in controllers)
            {
                i.Deinitialize();
            }

            foreach (var i in levelStateReporters)
            {
                i.OnLevelStateChanged -= Controller_OnLevelStateChanged;
            }
        }


        #endregion



        #region IInitialStateReturn

        public void ReturnToInitialState()
        {
            foreach(var i in stateReturnableControllers)
            {
                i.ReturnToInitialState();
            }
        }

        #endregion



        #region Ctor

        public LevelControllerService(ILevelEnvironment levelEnvironment,
                                      IPlayerStatisticService playerStatisticService)
        {
            Projectile = new LevelProjectileController();

            Target = new LevelTargetController();
            Path = new LevelPathController(Target);
            PhysicalObjects = new LevelPhysicalObjectsController();
            Announcer = new LevelAnnouncersController();
            SkullAnnouncer = new LevelSkullAnnouncersController();
            MotionWin = new LevelMotionWin();
            Stage = new LevelStageController();
            ExtraCurrency = new LevelExtraCurrencyController(playerStatisticService);
            CurrencyCollector = new LevelCurrencyCollectController();
            LevelPetCollector = new BonusLevelPetCollectController();
            SkullCollector = new LevelSkullCollectController();
            ProjectilesSound = new LevelSoundProjectileController();
            BonusLevelController = new BonusLevelController(levelEnvironment);
            ShootersInput = new ShootersInputLevelController(Path, BonusLevelController);
            UsualDrawController = new DrawModeUsualLevelController(levelEnvironment);
            BossDrawController = new DrawModeBossLevelController(levelEnvironment);
            BonusDrawController = new DrawModeBonusLevelController(levelEnvironment);
            LineInput = new ShootersHitmastersInputLevelController();
            LevelCurrencyAnnouncers = new LevelCurrencyAnnouncersController();
            PetsInputLevelController = new PetsInputLevelController();
            CurrencyObjectsLevelController = new CurrencyObjectsLevelController();
            SoulTrailController = new LevelSoulTrailController();

            controllers = new List<ILevelController>
            {
                MotionWin,
                Projectile,
                Target,
                Announcer,
                SkullAnnouncer,
                ExtraCurrency,
                ShootersInput,
                CurrencyCollector,
                LevelPetCollector,
                SkullCollector,
                ProjectilesSound,
                Path,
                Stage,
                BonusLevelController,
                PhysicalObjects,
                UsualDrawController,
                BossDrawController,
                BonusDrawController,
                LineInput,
                LevelCurrencyAnnouncers,
                PetsInputLevelController,
                CurrencyObjectsLevelController,
                SoulTrailController
            };

            stateReturnableControllers = controllers.OfType<IInitialStateReturn>().ToList();

            levelStateReporters = controllers.OfType<ILevelStateChangeReporter>().ToList();            
        }

        #endregion



        #region Events handlers

        private void Controller_OnLevelStateChanged(LevelState state) =>
            OnLevelStateChanged?.Invoke(state);
        
        #endregion
    }
}

