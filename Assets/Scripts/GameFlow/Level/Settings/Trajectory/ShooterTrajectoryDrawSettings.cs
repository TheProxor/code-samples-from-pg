using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "ShooterTrajectoryDrawSettings",
                        menuName = NamingUtility.MenuItems.IngameSettings + "ShooterTrajectoryDrawSettings")]
    public class ShooterTrajectoryDrawSettings : TrajectoryDrawSettings
    {
        public float petsTrajectoryClearDuration = default;
    }
}