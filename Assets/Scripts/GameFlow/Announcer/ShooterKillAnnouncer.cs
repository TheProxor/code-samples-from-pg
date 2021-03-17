using DG.Tweening;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using UnityEngine;

namespace Drawmasters.Announcer
{
    public class ShooterKillAnnouncer : Announcer
    {
        #region Fields
        
        protected override VectorAnimation AnnouncerAnimation => IngameData.Settings.announcerSettings.commonAnnouncer;
        private readonly bool isEnabled;

        #endregion



        #region Ctor

        public ShooterKillAnnouncer(Transform animatable): base(animatable)
        {
            isEnabled = GameServices.Instance.LevelEnvironment.Context.IsBossLevel;
            if (isEnabled)
            {
                Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            }
        }

        #endregion



        #region IDeinitializable
        
        public override void Deinitialize()
        {
            if (isEnabled)
            {
                Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            }

            DOTween.Kill(this);
        }
        
        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState levelState)
        {
            if (levelState == LevelState.FriendlyDeath)
            {
                Ready(this);
            }
        }

        #endregion
    }
}
