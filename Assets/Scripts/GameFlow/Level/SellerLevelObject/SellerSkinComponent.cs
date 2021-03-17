using System.Collections;
using System.Collections.Generic;
using Drawmasters.Utils;
using Spine;
using Spine.Unity;


namespace Drawmasters.Levels
{
    public class SellerSkinComponent : LevelObjectComponentTemplate<SellerLevelObject>
    {
        private readonly string mainSkin;
        private readonly string[] allowedWeaponSkins;

        public SellerSkinComponent(string _mainSkin, string[] _allowedWeaponSkins)
        {
            mainSkin = _mainSkin;
            allowedWeaponSkins = _allowedWeaponSkins;
        }

        public override void Enable()
        {
            SkeletonAnimation savedSkeletonAnimation = levelObject.SkeletonAnimation;
            Skin mainFoundSkin = SpineUtility.FindSkin(mainSkin, in savedSkeletonAnimation);

            string weaponSkinName = allowedWeaponSkins.RandomObject();
            Skin weaponFoundSkin = SpineUtility.FindSkin(weaponSkinName, in savedSkeletonAnimation);

            if (mainFoundSkin == null ||
                weaponFoundSkin == null)
            {
                CustomDebug.Log($"No combined skins found in {this}");
                return;
            }

            Skin combined = new Skin("main_and_weapon_skin");

            combined.AddSkin(mainFoundSkin);
            combined.AddSkin(weaponFoundSkin);

            levelObject.SkeletonAnimation.Initialize(true);
            levelObject.SkeletonAnimation.skeleton.SetSkin(combined);
            levelObject.SkeletonAnimation.skeleton.SetToSetupPose();
        }


        public override void Disable()
        {
        }
    }
}
