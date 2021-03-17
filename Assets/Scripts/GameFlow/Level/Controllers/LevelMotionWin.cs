using UnityEngine;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public class LevelMotionWin : ILevelController
    {
        #region Methods

        public void Initialize()
        {
            //IngameCamera.Instance.ClearAll();
            TimeUtility.Clear();

            MotionEffectLevelTargetComponent.OnShouldPlayEffect += MotionEffectLevelTargetComponent_OnShouldPlayEffect;
        }


        public void Deinitialize()
        {
            IngameCamera.Instance.ClearAll();
            TimeUtility.Clear();

            MotionEffectLevelTargetComponent.OnShouldPlayEffect -= MotionEffectLevelTargetComponent_OnShouldPlayEffect;
        }


        public void PlayAction(LevelTarget focusEnemy)
        {
            LevelWinMotionSettings.Zoom zoom = IngameData.Settings.levelWinMotionSettings.zoom;

            IngameCamera.Instance.Zoom(zoom.zoomFactor, zoom.duration, zoom.curve);


            LevelWinMotionSettings.CameraMove move = IngameData.Settings.levelWinMotionSettings.cameraMove;

            Vector3 focusWorlPoint = IngameCamera.Instance.Camera.ViewportToWorldPoint(move.cameraRelative);
            Vector3 localOffset = focusWorlPoint - IngameCamera.Instance.Camera.transform.position;
            Vector3 cameraEndPosition = focusEnemy.FocusPostion - localOffset;
            Vector3 cameraLocal = cameraEndPosition - IngameCamera.Instance.Camera.transform.position;

            cameraLocal.z = IngameCamera.Instance.Camera.transform.localPosition.z;

            IngameCamera.Instance.MoveLocal(cameraLocal, move.duration, move.curve);

            PlaySlowmotion();
        }


        private void PlaySlowmotion()
        {
            LevelWinMotionSettings.SlowMotion[] slowmo = IngameData.Settings.levelWinMotionSettings.slowMotion;

            TimeUtility.PlaySlowmoSequence(slowmo, this, () => TimeUtility.Clear());
        }

        #endregion



        #region Events handlers

        private void MotionEffectLevelTargetComponent_OnShouldPlayEffect(LevelTarget focusEnemy)
        {
            PlaySlowmotion();
        }
            

        #endregion
    }
}

