using Drawmasters.Levels;
using Drawmasters.Levels.Inerfaces;
using Drawmasters.Pets;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface ILevelControllerService : IInitializable, IDeinitializable, IInitialStateReturn, ILevelStateChangeReporter
    {
        LevelProjectileController Projectile { get; }
        LevelTargetController Target { get; }
        LevelPhysicalObjectsController PhysicalObjects { get; }
        LevelAnnouncersController Announcer { get; }
        LevelMotionWin MotionWin { get; }
        LevelStageController Stage { get; }
        LevelExtraCurrencyController ExtraCurrency { get; }
        ShootersInputLevelController ShootersInput { get; }
        LevelCurrencyCollectController CurrencyCollector { get; }
        BonusLevelPetCollectController LevelPetCollector { get; }
        LevelSoundProjectileController ProjectilesSound { get; }
        
        LevelPathController Path { get; }
        
        BonusLevelController BonusLevelController { get; }

        ShootersHitmastersInputLevelController LineInput { get; }

        LevelCurrencyAnnouncersController LevelCurrencyAnnouncers { get; }

        PetsInputLevelController PetsInputLevelController { get; }

        CurrencyObjectsLevelController CurrencyObjectsLevelController { get; }
        
        LevelSkullCollectController SkullCollector { get; }
        
        LevelSoulTrailController SoulTrailController { get; }
    }
}
