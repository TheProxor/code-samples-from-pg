using Spine.Unity;


namespace Drawmasters.LevelConstructor
{
    public class EditorSimpleLevelTargetObject : EditorLevelObject
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
    }
}

