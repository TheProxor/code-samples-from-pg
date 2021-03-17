using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using UnityEngine;

namespace Drawmasters.Announcer
{
    public class BonusLevelStartDrawAnnouncer : Announcer
    {
        #region Fields

        private readonly BonusLevelController controller;

        #endregion



        #region Ctor

        public BonusLevelStartDrawAnnouncer(Transform animatable) : base(animatable)
        {
            controller = GameServices.Instance.LevelControllerService.BonusLevelController;
            controller.OnDecelerationBegin += Controller_OnDecelerationBegin;
        }

        #endregion



        #region Abstract implementation

        protected override VectorAnimation AnnouncerAnimation => IngameData.Settings.announcerSettings.bonusLevelStartDrawAnnouncer;

        public override void Deinitialize()
        {
            controller.OnDecelerationBegin -= Controller_OnDecelerationBegin;
        }

        #endregion



        #region Events handlers

        private void Controller_OnDecelerationBegin(int stageIndex)
        {
            Ready(this);
        }

        #endregion
    }
}
