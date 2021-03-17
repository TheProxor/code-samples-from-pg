using System;


namespace Drawmasters.Prefs
{
    [Serializable]
    public class ShooterSkinInfo : HoldInfo<WeaponType>
    {
        public ShooterSkinType skinType = default;
    }
}