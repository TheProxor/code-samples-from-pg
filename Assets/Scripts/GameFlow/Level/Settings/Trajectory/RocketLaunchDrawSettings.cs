using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "RocketLaunchDrawSettings",
                        menuName = NamingUtility.MenuItems.IngameSettings + "RocketLaunchDrawSettings")]
    public class RocketLaunchDrawSettings : TrajectoryDrawSettings
    {
        #region Fields

        [Header("Boss Settings")]

        public float stepDistance = default;
        public float fullTrajectoryDrawSpeed = default;

        #endregion
    }
}
