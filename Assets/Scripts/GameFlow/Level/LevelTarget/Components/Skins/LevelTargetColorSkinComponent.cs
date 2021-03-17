using Spine;


namespace Drawmasters.Levels
{
    public class LevelTargetColorSkinComponent : LevelTargetComponent
    {
        #region Methods

        public override void Enable()
        {
            if(levelTarget.ShouldLoadColorData)
            {
                RefreshSkin();
            }
        }
        

        public override void Disable() { }


        private void RefreshSkin()
        {
            string shooterSkinName = IngameData.Settings.levelTargetSkinsSettings.FindColorSkin(levelTarget.ColorType);
            Skin characterFoundSkin = FindSkin(shooterSkinName);

            if (characterFoundSkin == null)
            {
                CustomDebug.Log($"Can't set skin type {levelTarget.SkeletonAnimation.skeletonDataAsset}" +
                    $"\nfor {levelTarget.SkeletonAnimation.Skeleton} string {shooterSkinName}.");
                return;
            }

            levelTarget.SkeletonAnimation.Initialize(true);
            levelTarget.SkeletonAnimation.skeleton.SetSkin(characterFoundSkin);
            levelTarget.SkeletonAnimation.skeleton.SetToSetupPose();
        }


        private Skin FindSkin(string name)
        {
            if (name == null)
            {
                return null;
            }

            Skin result = levelTarget.SkeletonAnimation.Skeleton.Data.FindSkin(name);
            return result;
        }

        #endregion
    }
}
