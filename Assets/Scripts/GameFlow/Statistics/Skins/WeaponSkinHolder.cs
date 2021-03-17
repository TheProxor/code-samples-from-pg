using Drawmasters.Levels;

namespace Drawmasters.Prefs
{
    public class WeaponSkinHolder : InfoHolder<WeaponSkinInfo, WeaponType>
    {
        private WeaponSkinType weaponSkinType;

        public WeaponSkinHolder(string _prefsKey) :
            base(_prefsKey)
        {
            
        }
    }
}
