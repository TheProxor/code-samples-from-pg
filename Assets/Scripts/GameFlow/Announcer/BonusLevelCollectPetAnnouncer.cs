using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using UnityEngine;

namespace Drawmasters.Announcer
{
    public class BonusLevelCollectPetAnnouncer : BonusLevelFinishedAnnouncer
    {
        #region Properties

        protected override bool CanShowAnnouncer =>
            GameServices.Instance.LevelControllerService.LevelPetCollector.IsAnyPetCollected;

        #endregion


        #region Ctor

        public BonusLevelCollectPetAnnouncer(Transform animatable) : base(animatable) { }

        #endregion
    }
}
