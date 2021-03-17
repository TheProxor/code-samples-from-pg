using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui;
using UnityEngine;

namespace Drawmasters.Announcer
{
    public class BonusLevelAnnouncer : Announcer
    {
        #region Fields
        
        private readonly bool isEnabled;
        private readonly AnimatorView view;

        #endregion



        #region Abstract implementation

        protected override VectorAnimation AnnouncerAnimation => IngameData.Settings.announcerSettings.bonusLevelAnnouncer;

        #endregion



        #region Ctor

        public BonusLevelAnnouncer(AnimatorView animatorView, Transform animatable): base(animatable)
        {
            view = animatorView;
            
            isEnabled = GameServices.Instance.LevelEnvironment.Context.IsFirstSublevel;
            isEnabled &= GameServices.Instance.LevelEnvironment.Context.IsBonusLevel;
            if (isEnabled)
            {
                view.OnShowEnd += View_OnShowEnd;
            }
        }

        #endregion
        
        
        
        #region Methods
        
        public override void Show()
        {
            base.Show();
            
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIBonusPanelAppear, animatedObject.transform.position);
        }
        
        #endregion
        
        
        
        #region IDeinitializable
        
        public override void Deinitialize()
        {
            if (isEnabled)
            {
                view.OnShowEnd -= View_OnShowEnd;
            }
        }

        #endregion

        

        #region Events handlers
        
        private void View_OnShowEnd(AnimatorView shownView)
        {
            Ready(this);
        }
        
        #endregion
    }
}

