using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public class ShooterTapZoneDisableComponent : ShooterComponent
    {
        #region Fields

        private readonly ShooterTapZone shooterTapZone;

        #endregion



        #region Ctor

        public ShooterTapZoneDisableComponent(ShooterTapZone _shooterTapZone)
        {
            shooterTapZone = _shooterTapZone;
        }

        #endregion




        #region Overrides

        public override void StartGame()
        {
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;
            bool isTapZoneEnabled = !context.Mode.IsHitmastersLiveOps();

            CommonUtility.SetObjectActive(shooterTapZone.gameObject, isTapZoneEnabled);
        }


        public override void Deinitialize()
        {
            CommonUtility.SetObjectActive(shooterTapZone.gameObject, true);
        }

        #endregion
    }
}

