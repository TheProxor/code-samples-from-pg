using DG.Tweening;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui;
using UnityEngine;


namespace Drawmasters.Announcer
{
    public class HostageLevelAnnouncer : Announcer
    {
        #region Fields

        protected override VectorAnimation AnnouncerAnimation => IngameData.Settings.announcerSettings.commonAnnouncer;
        private readonly bool isEnabled;
        private readonly IngameScreen ingameScreen;

        #endregion



        #region Ctor

        public HostageLevelAnnouncer(IngameScreen screen, Transform animatable): base(animatable)
        {
            ingameScreen = screen;

            isEnabled = GameServices.Instance.LevelEnvironment.Context.IsHostagesLevel;
            if (isEnabled)
            {
                ingameScreen.OnShowBegin += IngameScreen_OnShowBegin;
            }
        }
        
        #endregion



        #region IDeinitializable
        
        public override void Deinitialize()
        {
            if (isEnabled)
            {
                ingameScreen.OnShowBegin -= IngameScreen_OnShowBegin;
            }

            DOTween.Kill(this);
        }

        #endregion



        #region Events handlers

        private void IngameScreen_OnShowBegin(AnimatorView view)
        {
            Ready(this);
        }

        #endregion
    }
}

