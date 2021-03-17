using System;
using System.Collections.Generic;
using DG.Tweening;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class LinkedMarkerShowHideComponent : LevelObjectComponentTemplate<LinkedMarkerLevelObject>
    {
        #region Fields

        public static event Action<LinkedMarkerLevelObject, List<LevelObject>> OnShouldStartMonitor;

        #endregion



        #region Methods

        public override void Initialize(LinkedMarkerLevelObject _levelObject)
        {
            base.Initialize(_levelObject);

            // Do this on Initialize because of SetLinks lifecycle
            levelObject.OnLinksSet += LevelObject_OnLinksSet;
        }


        public override void Enable()
        {
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            LinkedMarkerMonitorComponent.OnShouldHide += LinkedMarkerMonitorComponent_OnShouldHide;
        }


        public override void Disable()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            LinkedMarkerMonitorComponent.OnShouldHide -= LinkedMarkerMonitorComponent_OnShouldHide;

            levelObject.OnLinksSet -= LevelObject_OnLinksSet;

            DOTween.Kill(this);
        }


        private void Show() =>
            levelObject.MainAnimator.SetTrigger(AnimationKeys.Screen.Show);


        private void Hide() =>
            levelObject.MainAnimator.SetTrigger(AnimationKeys.Screen.Hide);

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.Initialized)
            {
                Show();
            }
        }


        private void LinkedMarkerMonitorComponent_OnShouldHide(LinkedMarkerLevelObject anotherLevelObject, LevelObject monitoredLevelObject)
        {
            if (levelObject != anotherLevelObject)
            {
                return;
            }

            LinkedMarkerMonitorComponent.OnShouldHide -= LinkedMarkerMonitorComponent_OnShouldHide;

            Hide();
        }


        private void LevelObject_OnLinksSet(List<LevelObject> links) =>
            OnShouldStartMonitor?.Invoke(levelObject, links);

        #endregion
    }
}
