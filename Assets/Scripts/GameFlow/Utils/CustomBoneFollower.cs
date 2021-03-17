// Just like Spine BoneFollower.cs but allow to rid of following rotation on certain axis
#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif

using System;
using UnityEngine;

namespace Spine.Unity
{

    /// <summary>Sets a GameObject's transform to match a bone on a Spine skeleton.</summary>
#if NEW_PREFAB_SYSTEM
    [ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
    [AddComponentMenu("Spine/CustomBoneFollower")]
    public class CustomBoneFollower : MonoBehaviour
    {

        #region Inspector
        public SkeletonRenderer skeletonRenderer;
        public SkeletonRenderer SkeletonRenderer
        {
            get { return skeletonRenderer; }
            set
            {
                skeletonRenderer = value;
                Initialize();
            }
        }

        /// <summary>If a bone isn't set in code, boneName is used to find the bone at the beginning. For runtime switching by name, use SetBoneByName. You can also set the BoneFollower.bone field directly.</summary>
        [SpineBone(dataField: "skeletonRenderer")]
        [SerializeField] public string boneName;

        public bool followZPosition = true;

        public bool followBoneRotation = true;
        public bool followBoneRotationX = true;
        public bool followBoneRotationY = true;
        public bool followBoneRotationZ = true;

        [Tooltip("Follows the skeleton's flip state by controlling this Transform's local scale.")]
        public bool followSkeletonFlip = true;

        [Tooltip("Follows the target bone's local scale. BoneFollower cannot inherit world/skewed scale because of UnityEngine.Transform property limitations.")]
        public bool followLocalScale = false;

        [UnityEngine.Serialization.FormerlySerializedAs("resetOnAwake")]
        public bool initializeOnAwake = true;
        #endregion

        [NonSerialized] public bool valid;
        [NonSerialized] public Bone bone;

        Transform skeletonTransform;
        bool skeletonTransformIsParent;

        /// <summary>
        /// Sets the target bone by its bone name. Returns false if no bone was found. To set the bone by reference, use BoneFollower.bone directly.</summary>
        public bool SetBone(string name)
        {
            bone = skeletonRenderer.skeleton.FindBone(name);
            if (bone == null)
            {
                Debug.LogError("Bone not found: " + name, this);
                return false;
            }
            boneName = name;
            return true;
        }

        public void Awake()
        {
            if (initializeOnAwake) Initialize();
        }

        public void HandleRebuildRenderer(SkeletonRenderer skeletonRenderer)
        {
            Initialize();
        }

        public void Initialize()
        {
            bone = null;
            valid = skeletonRenderer != null && skeletonRenderer.valid;
            if (!valid) return;

            skeletonTransform = skeletonRenderer.transform;
            skeletonRenderer.OnRebuild -= HandleRebuildRenderer;
            skeletonRenderer.OnRebuild += HandleRebuildRenderer;
            skeletonTransformIsParent = Transform.ReferenceEquals(skeletonTransform, transform.parent);

            if (!string.IsNullOrEmpty(boneName))
                bone = skeletonRenderer.skeleton.FindBone(boneName);

#if UNITY_EDITOR
            if (Application.isEditor)
                LateUpdate();
#endif
        }

        void OnDestroy()
        {
            if (skeletonRenderer != null)
                skeletonRenderer.OnRebuild -= HandleRebuildRenderer;
        }

        public void LateUpdate()
        {
            if (!valid)
            {
                Initialize();
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
                skeletonTransformIsParent = Transform.ReferenceEquals(skeletonTransform, transform.parent);
#endif

            if (bone == null)
            {
                if (string.IsNullOrEmpty(boneName)) return;
                bone = skeletonRenderer.skeleton.FindBone(boneName);
                if (!SetBone(boneName)) return;
            }

            Transform thisTransform = this.transform;
            if (skeletonTransformIsParent)
            {
                // Recommended setup: Use local transform properties if Spine GameObject is the immediate parent
                thisTransform.localPosition = new Vector3(bone.WorldX, bone.WorldY, followZPosition ? 0f : thisTransform.localPosition.z);
                if (followBoneRotation)
                {
                    float halfRotation = Mathf.Atan2(bone.C, bone.A) * 0.5f;
                    if (followLocalScale && bone.ScaleX < 0) // Negate rotation from negative scaleX. Don't use negative determinant. local scaleY doesn't factor into used rotation.
                        halfRotation += Mathf.PI * 0.5f;

                    var q = default(Quaternion);
                    q.z = Mathf.Sin(halfRotation);
                    q.w = Mathf.Cos(halfRotation);
                    thisTransform.localRotation = q;
                }
            }
            else
            {
                // For special cases: Use transform world properties if transform relationship is complicated
                Vector3 targetWorldPosition = skeletonTransform.TransformPoint(new Vector3(bone.WorldX, bone.WorldY, 0f));
                if (!followZPosition) targetWorldPosition.z = thisTransform.position.z;

                float boneWorldRotation = bone.WorldRotationX;

                Transform transformParent = thisTransform.parent;
                if (transformParent != null)
                {
                    Matrix4x4 m = transformParent.localToWorldMatrix;
                    if (m.m00 * m.m11 - m.m01 * m.m10 < 0) // Determinant2D is negative
                        boneWorldRotation = -boneWorldRotation;
                }

                if (followBoneRotation)
                {
                    Vector3 worldRotation = skeletonTransform.rotation.eulerAngles;
                    if (followLocalScale && bone.ScaleX < 0) boneWorldRotation += 180f;

                    Vector3 worldRotationToFollow = default;
                    worldRotationToFollow.x = followBoneRotationX ? worldRotation.x : 0.0f;
                    worldRotationToFollow.x = followBoneRotationY ? worldRotation.y : 0.0f;
                    worldRotationToFollow.x = followBoneRotationZ ? worldRotation.z : 0.0f;

                    thisTransform.SetPositionAndRotation(targetWorldPosition, Quaternion.Euler(worldRotationToFollow.x, worldRotationToFollow.y, worldRotationToFollow.z + boneWorldRotation));
                }
                else
                {
                    thisTransform.position = targetWorldPosition;
                }
            }

            Vector3 localScale = followLocalScale ? new Vector3(bone.ScaleX, bone.ScaleY, 1f) : new Vector3(1f, 1f, 1f);
            if (followSkeletonFlip) localScale.y *= Mathf.Sign(bone.Skeleton.ScaleX * bone.Skeleton.ScaleY);
            thisTransform.localScale = localScale;
        }
    }

}

