using System;
using DG.Tweening;
using Drawmasters.Announcer;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Drawmasters.Ui
{
    public class BossBehaviour : BaseIngameScreenBehaviour
    {
        #region Helpers

        [Serializable]
        public class BossData
        {
            [SerializeField][Required] public StagesUiHandler stagesHandler = default;
            [SerializeField][Required] public Transform bossLevelAnimatable = default;
            [SerializeField][Required] public Transform bossLevelDefeatAnimatable = default;
            [Required] public Transform heroKilledAnimatable = default;
            
            [SerializeField][Required] public CanvasGroup bossStageCanvasGroup = default;
            [SerializeField][Required] public FactorAnimation bossStageFadeAnimation = default;
            [SerializeField][Required] public FactorAnimation bossStageAppearAnimation = default;
        }

        #endregion
        
        
        
        #region Fields

        private readonly BossData bossData;
        private readonly LevelStageController stageController;
        
        #endregion
        
        
        
        #region Ctor

        public BossBehaviour(IngameScreen ingameScreen, 
                             Data _data, 
                             BossData _bossData) : 
            base(ingameScreen, _data)
        {
            bossData = _bossData;
            stageController = GameServices.Instance.LevelControllerService.Stage;
        }
        
        #endregion
        
        
        
        #region Abstract implementation

        protected override Announcer.Announcer[] AvailableAnnouncers
            => new Announcer.Announcer[]
            {
                new BossLevelAnnouncer(screen, bossData.bossLevelAnimatable),
                new BossDefeatedAnnouncer(bossData.bossLevelDefeatAnimatable),
                new ShooterKillAnnouncer(bossData.heroKilledAnimatable)
            };

        protected override bool IsSkipButtonAvailable => true;

        public override bool IsCallPetButtonAvailable => false;

        #endregion



        #region Overrided methods

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

