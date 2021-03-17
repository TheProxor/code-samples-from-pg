using DG.Tweening;
using Drawmasters.Announcer;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using static Drawmasters.Ui.BossBehaviour;


namespace Drawmasters.Ui
{
    public class LiveOpsBossIngameScreenBehaviour : LiveOpsIngameScreenBehaviour
    {
        #region Fields

        private readonly BossData bossData;
        private readonly LevelStageController stageController;

        #endregion



        #region Ctor

        public LiveOpsBossIngameScreenBehaviour(IngameScreen ingameScreen, 
                                                BaseIngameScreenBehaviour.Data _data, 
                                                LiveOpsIngameScreenBehaviour.LiveOpsData _liveOpsData,
                                                BossBehaviour.BossData _bossData)
            : base(ingameScreen, _data, _liveOpsData)
        {
            bossData = _bossData;
            stageController = GameServices.Instance.LevelControllerService.Stage;
        }

        #endregion



        #region Overrided

        protected override Announcer.Announcer[] AvailableAnnouncers
            => new Announcer.Announcer[]
            {
                new BossLevelAnnouncer(screen, bossData.bossLevelAnimatable),
                new BossDefeatedAnnouncer(bossData.bossLevelDefeatAnimatable),
                new ShooterKillAnnouncer(bossData.heroKilledAnimatable)
            };


        protected override void OnLevelReloaded()
        {
            stageController.Deinitialize();

            base.OnLevelReloaded();

            stageController.Initialize();
        }


        public override void Initialize()
        {
            base.Initialize();

            bossData.stagesHandler.Initialize();

            stageController.OnStartChangeStage += LevelStageController_OnStartChangeStage;
            stageController.OnFinishChangeStage += LevelStageController_OnFinishChangeStage;
            stageController.OnStartChangeStage += OnStartChangeStage_ResetSkipButtonAnimation;
        }


        public override void Deinitialize()
        {
            bossData.stagesHandler.Deinitialize();

            stageController.OnStartChangeStage -= LevelStageController_OnStartChangeStage;
            stageController.OnFinishChangeStage -= LevelStageController_OnFinishChangeStage;
            stageController.OnStartChangeStage -= OnStartChangeStage_ResetSkipButtonAnimation;

            DOTween.Kill(this);

            base.Deinitialize();
        }

        #endregion


        #region Events handlers

        private void LevelStageController_OnStartChangeStage()
        {
            DOTween.Complete(this);

            bossData.bossStageFadeAnimation.Play((value) => bossData.bossStageCanvasGroup.alpha = value, this);
        }


        private void LevelStageController_OnFinishChangeStage()
        {
            DOTween.Complete(this);

            bossData.bossStageAppearAnimation.Play((value) => bossData.bossStageCanvasGroup.alpha = value, this);
        }


        private void OnStartChangeStage_ResetSkipButtonAnimation()
        {
            skipLevelButtonAnimator.ResetTrigger(data.skipLevelAnimationButton.animationTriggers.normalTrigger);
            skipLevelButtonAnimator.SetTrigger(data.skipLevelAnimationButton.animationTriggers.normalTrigger);
        }

        #endregion
    }
}