using System.Collections;
using Drawmasters.Levels;
using Spine;
using Spine.Unity;


namespace Drawmasters.LevelConstructor
{
    public class EditorLevelTargetObject : EditorColorsLevelObject
    {
        #region Unity lifecycle

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



        #region Properties

        public bool ShouldUseColorTypeSkin => !EditorObjectsContainer.CurrentGameMode.IsHitmastersLiveOps();

        #endregion



        #region Methods

        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("LevelTargetInspectorExtension.prefab");

            
        public override void OnColorTypeRefresh(ShooterColorType shooterColorType)
        {
            base.OnColorTypeRefresh(shooterColorType);

            if (ShouldUseColorTypeSkin)
            {
                bool wasSelected = isSelected;

                if (isSelected)
                {
                    Deselect();
                }


                var animation = GetComponentInChildren<SkeletonAnimation>(true);

                string shooterSkinName = GetSkin(shooterColorType);
                Skin characterFoundSkin = FindSkin(shooterSkinName);

                if (characterFoundSkin == null)
                {
                    CustomDebug.Log($"Can't set skin type {animation.skeletonDataAsset}" +
                        $"\nfor {animation.Skeleton} string {shooterSkinName}.");
                    return;
                }

                animation.Initialize(true);
                animation.skeleton.SetSkin(characterFoundSkin);
                animation.skeleton.SetToSetupPose();
                animation.LateUpdate();

                Skin FindSkin(string name)
                {
                    if (name == null)
                    {
                        return null;
                    }

                    Skin result = animation.Skeleton.Data.FindSkin(name);
                    return result;
                }

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
            }

        }


        protected virtual string GetSkin(ShooterColorType shooterColorType) =>
            IngameData.Settings.levelTargetSkinsSettings.FindColorSkin(shooterColorType);

        #endregion
    }
}