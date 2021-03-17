using UnityEngine;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "IngameCameraSettings",
                        menuName = NamingUtility.MenuItems.IngameSettings + "IngameCameraSettings")]
    public class IngameCameraSettings : ScriptableObject
    {
        public float portraitCameraSize = default;
        public float landscapeCameraSize = default;
    }
}
