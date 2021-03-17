using UnityEngine;
using Drawmasters.Levels;
using Spine;
using System.Collections.Generic;
using System.Linq;


namespace Drawmasters
{
    public static class ExplosionUtility
    {
        #region Methods

        public static Vector2 ExplosionForce(Transform explosionTransform,
                                             Transform affectedTransform,
                                             DynamiteSettings.ExplosionData explosionData)
        {
            return GetExplosionForce(explosionTransform, affectedTransform, explosionData, explosionData.explosionForce);
        }

        public static Vector2 EnemyExplosionForce(Transform explosionTransform,
                                             Transform affectedTransform,
                                             DynamiteSettings.ExplosionData explosionData)
        {
            return GetExplosionForce(explosionTransform, affectedTransform, explosionData, explosionData.enemyExplosionForce);
        }
        
        private static Vector2 GetExplosionForce(Transform explosionTransform,
                                             Transform affectedTransform,
                                             DynamiteSettings.ExplosionData explosionData, 
                                             float explosionForce)
        {
            Vector2 distance = affectedTransform.position - explosionTransform.position;
            Vector2 direction = distance.normalized;
            float distanceValue = distance.magnitude;

            float forceInverseFactor = distanceValue / explosionData.radius;
            float forceFactor = 1f - forceInverseFactor;
            forceFactor = Mathf.Max(forceFactor, 0f);
            float forceMultiplier = explosionData.distanceDependence.Evaluate(forceFactor);

            float force = explosionForce * forceMultiplier;

            return direction * force;
        }
        
        public static void ApplyForce(Rigidbody2D reciever,
                                      Transform explosible,
                                      DynamiteSettings.ExplosionData data)
        {
            Vector2 force = ExplosionForce(explosible,
                                           reciever.transform,
                                           data);

            reciever.AddForce(force, ForceMode2D.Impulse);
        }


        public static void ApplyEnemyForce(Rigidbody2D reciever,
                                      Transform explosible,
                                      DynamiteSettings.ExplosionData data)
        {
            Vector2 force = EnemyExplosionForce(explosible,
                                           reciever.transform,
                                           data);

            reciever.AddForce(force, ForceMode2D.Impulse);
        }


        public static void ExplodeLimb(string boneName, LevelTarget levelTarget)
        {
            LevelTargetLimb explosiveLimb = levelTarget.Limbs.Find(limb =>
                limb.RootBoneName.Equals(boneName));

            if (explosiveLimb == null)
            {
                CustomDebug.Log($"Not found explosive limb with name: {boneName}");
                return;
            }

            BoneData rootBone = levelTarget.SkeletonAnimation.Skeleton.Data.FindBone(explosiveLimb.RootBoneName);
            if (rootBone == null)
            {
                CustomDebug.Log($"Not found data with name = {boneName}, {explosiveLimb.RootBoneName}");
                return;
            }

            HashSet<BoneData> otherRootBones = new HashSet<BoneData>();
            foreach (var limb in levelTarget.Limbs)
            {
                if (limb != explosiveLimb)
                {
                    Bone bone = levelTarget.SkeletonAnimation.Skeleton.FindBone(limb.RootBoneName);

                    otherRootBones.AddIfNotContains(bone.Data);
                }
            }

            HashSet<BoneData> bones = new HashSet<BoneData>();
            HashSet<Slot> slots = new HashSet<Slot>();

            FindBonesRecursively(ref bones,
                                 ref otherRootBones,
                                 rootBone,
                                 levelTarget);
            FindSlots(ref bones, ref slots, levelTarget);
            DisableSlots(ref slots, levelTarget);
            RemoveOtherJoints(explosiveLimb, levelTarget);
            DisableBonesPhysic(explosiveLimb, levelTarget);

            explosiveLimb.MarkLimbExploded();
            levelTarget.MarkLimbExploded(boneName);
        }


        private static void FindBonesRecursively(ref HashSet<BoneData> bones,
                                  ref HashSet<BoneData> stopBones,
                                  BoneData startBoneData,
                                  LevelTarget levelTarget)
        {
            bones.AddIfNotContains(startBoneData);

            foreach (var bone in levelTarget.SkeletonAnimation.Skeleton.Bones)
            {
                if (bone.Parent != null &&
                    bone.Parent.Data.Index == startBoneData.Index &&
                    !stopBones.Contains(bone.Data))
                {
                    FindBonesRecursively(ref bones, ref stopBones, bone.Data, levelTarget);
                }
            }
        }
        
        private static void FindSlots(ref HashSet<BoneData> bones, ref HashSet<Slot> slots, LevelTarget levelTarget)
        {
            var boneNames = bones.Select(bone => bone.Name);
            
            ExposedList<Slot> boneSlots = levelTarget.SkeletonAnimation.Skeleton.Slots.FindAll(slot =>
                boneNames.Contains(slot.Bone.Data.Name));

            slots.AddRange(boneSlots);
        }


        private static void DisableSlots(ref HashSet<Slot> slots, LevelTarget levelTarget)
        {
            foreach (var slot in slots)
            {
                levelTarget.SkeletonAnimation.Skeleton.SetAttachment(slot.Data.Name, null);
            }
        }


        private static void DisableBonesPhysic(LevelTargetLimb limb, LevelTarget levelTarget)
        {
            foreach (var limbPart in limb.LimbParts)
            {
                if (limbPart.MainCollider != null)
                {
                    limbPart.MainCollider.enabled = false;
                }

                Rigidbody2D limbPartRb = levelTarget.Ragdoll2D.GetRigidbody(limbPart.BoneName);

                if (limbPartRb != null)
                {
                    limbPartRb.simulated = false;

                    // TODO optimize
                    Joint2D joint = limbPartRb.GetComponent<Joint2D>();
                    if (joint != null)
                    {
                        joint.enabled = false;
                    }

                    CommonUtility.SetObjectActive(limbPartRb.gameObject, false);
                }
            }
        }


        private static void RemoveOtherJoints(LevelTargetLimb limb, LevelTarget levelTarget)
        {
            Rigidbody2D mainRigidbody = levelTarget.Ragdoll2D.GetRigidbody(limb.RootBoneName);
            if (mainRigidbody == null)
            {
                return;
            }

            foreach (var rb in levelTarget.Ragdoll2D.RigidbodyArray)
            {
                if (rb == null)
                {
                    continue;
                }

                Joint2D joint = rb.GetComponent<Joint2D>();
                if (joint != null &&
                    joint.attachedRigidbody == mainRigidbody)
                {
                    joint.enabled = false;
                }
            }
        }

        #endregion
    }
}
