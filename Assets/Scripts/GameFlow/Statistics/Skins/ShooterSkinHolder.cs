using UnityEngine;


namespace Drawmasters.Prefs
{
    public class ShooterSkinHolder : InfoHolder<ShooterSkinInfo, WeaponType>
    {
        private ShooterSkinType shooterSkinType;

        public ShooterSkinHolder(string _prefsKey) :
            base(_prefsKey)
        {

        }
    }
}
