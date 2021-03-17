using UnityEngine;
using System;
using Drawmasters.Effects;
using Bone = Spine.Bone;
using Drawmasters.Utils;

namespace Drawmasters.Levels
{
    public class BossShotFxComponent : LevelTargetComponent
    {
        #region Fields

        private readonly (string, ShooterColorType)[] shotBonesInfo;

        #endregion



        #region Class lifecycle

        public BossShotFxComponent((string, ShooterColorType)[] _shotBonesInfo)
        {
            shotBonesInfo = _shotBonesInfo;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            LevelTargetRocketLaunchComponent.OnRocketsLaunched += LevelTargetRocketLaunchComponent_OnRocketsLaunched;
        }


        public override void Disable()
        {
            LevelTargetRocketLaunchComponent.OnRocketsLaunched -= LevelTargetRocketLaunchComponent_OnRocketsLaunched;
        }

        #endregion



        #region Events handlers

        private void LevelTargetRocketLaunchComponent_OnRocketsLaunched(RocketLaunchData.Data[] data)
        {
            foreach (var d in data)
            {
                int foundInfoIndex = Array.FindIndex(shotBonesInfo, e => e.Item2 == d.colorType);

                if (foundInfoIndex != -1)
                {
                    (string, ShooterColorType) foundBoneInfo = shotBonesInfo.Find(e => e.Item2 == d.colorType);

                    Bone foundBone = levelTarget.SkeletonAnimation.Skeleton.FindBone(foundBoneInfo.Item1);

                    if (foundBone != null)
                    {
                        Vector3 boneWorldPosition = SpineUtility.BoneToWorldPosition(foundBone, levelTarget.SkeletonAnimation.transform);
                        Vector2 aimDirection = d.trajectory.FirstObject() - boneWorldPosition;
                        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

                        bool isFlipped = Mathf.Approximately(levelTarget.transform.rotation.eulerAngles.y, 180.0f);
                        if (isFlipped)
                        {
                            angle += 180.0f;
                        }

                        foundBone.Rotation = angle;
                        levelTarget.SkeletonAnimation.Update(default);

                        float fxAngle = isFlipped ? angle : angle - 180.0f;
                        EffectManager.Instance.PlaySystemOnce(EffectKeys.FxBossRocketLauncherShot, 
                            boneWorldPosition, 
                            Quaternion.Euler(Vector3.zero.SetZ(fxAngle)));
                    }
                }
            }
        }

        #endregion
    }
}
