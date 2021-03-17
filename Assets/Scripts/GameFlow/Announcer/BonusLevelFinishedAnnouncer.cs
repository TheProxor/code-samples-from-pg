using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using UnityEngine;

namespace Drawmasters.Announcer
{
    public class BonusLevelFinishedAnnouncer : Announcer
    {
        #region Fields

        private readonly BonusLevelController controller;

        #endregion



        #region Properties

        protected virtual bool CanShowAnnouncer =>
            !GameServices.Instance.LevelControllerService.LevelPetCollector.IsAnyPetCollected;

        #endregion



        #region Ctor

        public BonusLevelFinishedAnnouncer(Transform animatable) : base(animatable)
        {
            controller = GameServices.Instance.LevelControllerService.BonusLevelController;

            controller.OnBonusLevelFinished += Controller_OnBonusLevelFinished;
        }        

        #endregion



        #region Abstract implementation

        protected override VectorAnimation AnnouncerAnimation
            => IngameData.Settings.announcerSettings.bonusLevelFinishedAnnouncer;

        public override void Deinitialize()
        {
            controller.OnBonusLevelFinished -= Controller_OnBonusLevelFinished;
        }

        #endregion



        #region Events handlers

        private void Controller_OnBonusLevelFinished()
        {
            if (CanShowAnnouncer)
            {
                Ready(this);
            }
        }

        #endregion
    }
}
