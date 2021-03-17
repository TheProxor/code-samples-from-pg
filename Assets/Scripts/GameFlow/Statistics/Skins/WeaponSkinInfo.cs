using System;


namespace Drawmasters.Prefs
{
    [Serializable]
    public class WeaponSkinInfo : HoldInfo<WeaponType>
    {
        public WeaponSkinType skinType = default;
    }
}