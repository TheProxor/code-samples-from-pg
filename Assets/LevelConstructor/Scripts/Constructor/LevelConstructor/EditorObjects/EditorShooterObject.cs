using System.Collections;
using Drawmasters.Levels;
using Spine;
using Spine.Unity;


namespace Drawmasters.LevelConstructor
{
    public class EditorShooterObject : EditorColorsLevelObject
    {
        #region Fields

        protected override void Awake()
        {
            base.Awake();

            var animations = GetComponentsInChildren<SkeletonAnimation>(true);

            foreach (var skeletonAnimation in animations)
            {
                skeletonAnimation.Initialize(true);
                skeletonAnimation.LateUpdate();
            }
        }

        #endregion



        #region Methods

        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("LevelTargetInspectorExtension.prefab");

        public override void OnColorTypeRefresh(ShooterColorType shooterColorType)
        {
            base.OnColorTypeRefresh(shooterColorType);

            bool wasSelected = isSelected;

            if (isSelected)
            {
                Deselect();
            }

            var skeletonAnimation = GetComponentInChildren<SkeletonAnimation>(true);

            ShooterSkinType shooterSkinType = ShooterSkinType.Reddish;
            string shooterSkinName = IngameData.Settings.shooterSkinsSettings.GetAssetSkin(shooterSkinType, shooterColorType);

            Skin shooterFoundSkin = FindSkin(shooterSkinName);

            skeletonAnimation.Initialize(true);
            skeletonAnimation.skeleton.SetSkin(shooterFoundSkin);
            skeletonAnimation.skeleton.SetToSetupPose();

            StartCoroutine(UpdateSelection());

            IEnumerator UpdateSelection()
            {
                yield return null;
                yield return null;

                RefreshRenderers();

                if (!isSelected && wasSelected)
                {
                    Select();
                }
            }

            Skin FindSkin(string name)
            {
                if (name == null)
                {
                    return null;
                }

                Skin result = skeletonAnimation.Skeleton.Data.FindSkin(name);
                return result;
            }
        }

        #endregion
    }
}