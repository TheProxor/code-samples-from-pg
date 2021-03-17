using DG.Tweening;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui;
using UnityEngine;


namespace Drawmasters.Announcer
{
    public class BossLevelAnnouncer : Announcer
    {
        #region Fields

        protected override VectorAnimation AnnouncerAnimation => IngameData.Settings.announcerSettings.bossLevelAnnouncer;
        private readonly bool isEnabled;
        private readonly IngameScreen ingameScreen;

        #endregion



        #region Ctor

        public BossLevelAnnouncer(IngameScreen screen, Transform animatable): base(animatable)
        {
            ingameScreen = screen;
            isEnabled = GameServices.Instance.LevelEnvironment.Context.IsBossLevel;

            if (isEnabled)
            {
                ingameScreen.OnShowEnd += Screen_OnShowEnd;
            }
        }
        
        #endregion



        #region Methods
        
        public override void Show()
        {
            base.Show();
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIBossPanelAppear, animatedObject.transform.position);
        }

        #endregion


        
        #region IDeinitializable
        
        public override void Deinitialize()
        {
            if (isEnabled)
            {
                ingameScreen.OnShowEnd -= Screen_OnShowEnd;
            }

            DOTween.Kill(this);
        }

        #endregion
        


        #region Events handlers

        private void Screen_OnShowEnd(AnimatorView ingameScreen)
        {
            Ready(this);
        }

        #endregion
    }
}