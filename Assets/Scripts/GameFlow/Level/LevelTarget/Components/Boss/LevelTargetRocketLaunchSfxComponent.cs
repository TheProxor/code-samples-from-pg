using Modules.Sound;


namespace Drawmasters.Levels
{
    public class LevelTargetRocketLaunchSfxComponent : LevelTargetComponent
    {
        #region Methods

        public override void Enable()
        {
            LevelTargetRocketLaunchComponent.OnRocketsLaunched += LevelTargetRocketLaunchComponent_OnRocketsLaunched;
        }


        public override void Disable()
        {
            LevelTargetRocketLaunchComponent.OnRocketsLaunched -= LevelTargetRocketLaunchComponent_OnRocketsLaunched;
        }

        #endregion



        #region Events handlers

        private void LevelTargetRocketLaunchComponent_OnRocketsLaunched(RocketLaunchData.Data[] data) =>
            SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.BOSS_SHOT);
        
        #endregion
    }
}