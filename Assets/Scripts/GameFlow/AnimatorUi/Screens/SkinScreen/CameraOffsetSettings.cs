using UnityEngine;



namespace Drawmasters
{
    [CreateAssetMenu(fileName = "CameraOffsetSettings",
        menuName = NamingUtility.MenuItems.Settings + "CameraOffsetSettings")]
    public class CameraOffsetSettings : ScriptableObject
    {
        #region Fields

        public VectorAnimation animation = default;
        public float offsetY = default;

        #endregion
    }
}