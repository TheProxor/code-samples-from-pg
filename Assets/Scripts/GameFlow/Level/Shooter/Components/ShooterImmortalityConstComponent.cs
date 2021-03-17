namespace Drawmasters.Levels
{
    public class ShooterImmortalityConstComponent : ShooterComponent
    {
        #region Methods

        public override void StartGame()
        {
            shooter.SetImmortal(true);
        }


        public override void Deinitialize() { }

        #endregion
    }
}
