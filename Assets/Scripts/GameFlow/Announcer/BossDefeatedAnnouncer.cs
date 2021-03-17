using DG.Tweening;
using Drawmasters.Effects;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Announcer
{
    public class BossDefeatedAnnouncer : Announcer
    {
        #region Fields

        protected override VectorAnimation AnnouncerAnimation => IngameData.Settings.announcerSettings.bossDefeatAnnouncer;
        private readonly bool isEnabled;

        #endregion



        #region Ctor

        public BossDefeatedAnnouncer(Transform animatable): base(animatable)
        {
            isEnabled = GameServices.Instance.LevelEnvironment.Context.IsBossLevel;
            if (isEnabled)
            {
                Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            }
        }

        #endregion



        #region Methods

        public override void Show()
        {
            base.Show();
            
            animatedObject.localScale = animation.beginValue;
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIBossWinPanelEffect, animatedObject.transform.position);
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
            if (levelState == LevelState.AllTargetsHitted)
            {
                Ready(this);
            }
        }

        #endregion
    }
}
