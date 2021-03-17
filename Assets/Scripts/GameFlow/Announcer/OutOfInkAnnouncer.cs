using DG.Tweening;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using UnityEngine;

namespace Drawmasters.Announcer
{
    public class OutOfInkAnnouncer : Announcer
    {
        #region Fields

        protected override VectorAnimation AnnouncerAnimation => IngameData.Settings.announcerSettings.commonAnnouncer;
        private readonly LevelPathController pathController;

        #endregion
        
        
        
        #region Ctor

        public OutOfInkAnnouncer(Transform animatable): base(animatable)
        {
            pathController = GameServices.Instance.LevelControllerService.Path;
            pathController.OnPathChanged += PathController_OnPathChanged;
        }
        
        #endregion
        
        
        
        #region IDeinitializable

        public override void Deinitialize()
        {
            pathController.OnPathChanged -= PathController_OnPathChanged;
            DOTween.Kill(this);
        }

        #endregion
        
        
        
        #region Events handlers
        
        private void PathController_OnPathChanged(float value)
        {
            if (Mathf.Approximately(value, 1f))
            {
                Ready(this);
            }
        }
        
        #endregion
    }
}

