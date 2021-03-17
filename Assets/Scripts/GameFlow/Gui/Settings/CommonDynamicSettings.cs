using UnityEngine;


namespace Drawmasters.Ui
{
    [CreateAssetMenu(fileName = "CommonDynamicSettings",
                     menuName = NamingUtility.MenuItems.GuiSettings + "CommonDynamicSettings")]
    public class CommonDynamicSettings : ScriptableObject
    {
        #region Fields

        public VectorAnimation resultWeaponBounce = default;

        #endregion
    }
}

