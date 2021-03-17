using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity.Examples;
using Modules.Sound;
using DG.Tweening;
using Spine;
using Drawmasters.Effects;
using Modules.General;
using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;
#if UNITY_EDITOR
using System.Linq;
#endif

namespace Drawmasters.Levels
{
    public class LevelTargetLimb : MonoBehaviour, ILaserDestroyable
    {
        #region Fields

        public event Action OnExploded;
        public event Action<CollidableObject, LevelTargetLimb> OnCollidableObjectHitted;

        [SerializeField]
        [SpineBone(fallbackSearchInParents = true)]
        private string rootBoneName = default;

        [SerializeField] private LevelTargetLimbPart[] limbParts = default;

        [SerializeField] private List<DecalInfo> decalsInfo = default;

        private bool wasLaserDestroyStarted;

        #endregion



        #region Properties

        public LevelTarget ParentEnemy { get; private set; }

        public List<DecalInfo> DecalsInfo => decalsInfo;

        public string RootBoneName => rootBoneName;

        public List<LevelTargetLimbPart> LimbParts => new List<LevelTargetLimbPart>(limbParts);

        public List<string> PartsBonesNames
        {
            get
            {
                List<string> result = new List<string>(limbParts.Length);

                foreach (var part in limbParts)
                {
                    result.Add(part.BoneName);
                }

                return result;
            }
        }

        public bool IsChoppedOff { get; private set; }

        #endregion



        #region Methods

        public void Initialize(LevelTarget _parentEnemy)
        {
            ParentEnemy = _parentEnemy;

            foreach (var limbPart in limbParts)
            {
                limbPart.Initialize();
            }

            IsChoppedOff = false;
            wasLaserDestroyStarted = false;
        }


        public void ApplyDecals(SkeletonAnimation workAnimation)
        {
            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
            if (!playerData.IsBloodEnabled)
            {
                return;
            }

            if (decalsInfo != null)
            {
                decalsInfo.ForEach(decal => decal.ChangeAttachment(workAnimation));
            }
        }


        private void OnEnable()
        {
            foreach (var part in limbParts)
            {
                part.OnCollidableObjectHitted += Part_OnCollidableObjectHitted;
            }
        }


        private void OnDisable()
        {
            foreach (var part in limbParts)
            {
                part.OnCollidableObjectHitted -= Part_OnCollidableObjectHitted;
            }

            DOTween.Kill(this);

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public bool ContainLimbPart(string limbPartName)
        {
            bool result = default;

            foreach (var part in limbParts)
            {
                result = limbPartName == part.BoneName;

                if (result)
                {
                    break;
                }
            }

            return result;
        }


        public void ChopOff(SkeletonRagdoll2D ragdoll)
        {
            IsChoppedOff = true;

            Rigidbody2D limbRigidbody = ragdoll.GetRigidbody(rootBoneName);

            if (limbRigidbody == null)
            {
                return;
            }

            DisableJoint(limbRigidbody.gameObject);

            PlayChopOffVfx(limbRigidbody, ragdoll);
            PlayChopOffSfx();
        }


        private void DisableJoint(GameObject go)
        {
            Joint2D joint2D = go.GetComponent<Joint2D>();

            if (joint2D != null)
            {
                joint2D.enabled = false;
            }
        }


        private void PlayChopOffVfx(Rigidbody2D limbRigidbody, SkeletonRagdoll2D ragdoll)
        {
            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
            if (!playerData.IsBloodEnabled)
            {
                return;
            }

            LevelTargetSettings.LimbData limbData = IngameData.Settings.levelTargetSettings.FindLimbData(rootBoneName);

            if (limbData == null)
            {
                return;
            }

            string vfxName = limbData.chopOffEffectOnBodyKey;

            EffectManager.Instance.PlaySystemOnce(vfxName,
                                                  limbRigidbody.transform.position,
                                                  limbRigidbody.transform.rotation,
                                                  ragdoll.RootRigidbody.transform);

            Quaternion bone180DegRotated = Quaternion.Euler(-limbRigidbody.transform.eulerAngles);

            string limbVfxName = limbData.chopOffEffectOnLimbKey;

            EffectManager.Instance.PlaySystemOnce(limbVfxName,
                                                  limbRigidbody.transform.position,
                                                  bone180DegRotated,
                                                  limbRigidbody.transform);
        }


        public void MarkLimbExploded() =>
            OnExploded?.Invoke();


        private void PlayChopOffSfx() =>
           SoundManager.Instance.PlayOneShot(SoundGroupKeys.RandomLimbChopOffKey);


        public void StartLaserDestroy()
        {
            StartDestroy();

            void StartDestroy()
            {
                if (!wasLaserDestroyStarted)
                {
                    List<Slot> slots = new List<Slot>();
                    foreach (var part in PartsBonesNames)
                    {
                        slots.AddExclusive(ParentEnemy.GetEnabledSlot(part));
                    }

                    bool isChoppedOff = ParentEnemy.IsChoppedOffLimb(RootBoneName);

                    Sequence sequence = DOTween.Sequence();

                    if (isChoppedOff)
                    {
                        Color endCorroseColor = IngameData.Settings.laserSettings.laserHittedEndColor;

                        sequence
                            .Append(DOTween
                                    .To(() => Color.white,
                                        color => slots.ForEach((s) => s.SetColor(color)),
                                        endCorroseColor,
                                        IngameData.Settings.levelTarget.laserColorSetDuration)
                                    .SetEase(Ease.Linear)
                                    .SetId(this))
                            .Append(DOTween
                                    .To(() => endCorroseColor,
                                        color => slots.ForEach((s) => s.SetColor(color)),
                                        Color.clear,
                                        IngameData.Settings.levelTarget.laserClearColorSetDuration)
                                    .SetEase(Ease.Linear)
                                    .SetId(this));
                    }
                    else
                    {
                        sequence
                            .AppendInterval(IngameData.Settings.levelTarget.laserColorSetDuration)
                            .AppendInterval(IngameData.Settings.levelTarget.laserClearColorSetDuration);
                    }

                    sequence
                        .AppendCallback(() => ExplosionUtility.ExplodeLimb(RootBoneName, ParentEnemy))
                        .SetEase(Ease.Linear)
                        .SetId(this);

                    Scheduler.Instance.CallMethodWithDelay(this, () =>
                    {

                        LevelTargetSettings.LimbData foundData = ParentEnemy.Settings.FindLimbData(RootBoneName);
                        string effectKey = foundData == null ? string.Empty : foundData.laserDestroyedEffectKey;

                        EffectHandler effectHandler = EffectManager.Instance.PlaySystemOnce(effectKey,
                                                              transform.position,
                                                              transform.rotation);

                    }, IngameData.Settings.levelTarget.laserFxExplodeDelay);

                    wasLaserDestroyStarted = true;
                }
            }
        }

        #endregion



        #region Events handlers

        private void Part_OnCollidableObjectHitted(CollidableObject collidableObject, LevelTargetLimbPart part) =>
            OnCollidableObjectHitted?.Invoke(collidableObject, this);

        #endregion



        #region Editor methods

#if UNITY_EDITOR

        private void Reset()
        {
            AssignDefaultValues();
        }

        [Sirenix.OdinInspector.Button]
        private void AssignDefaultValues()
        {
            rootBoneName = GetComponent<SkeletonUtilityBone>().boneName;

            limbParts = gameObject.GetComponentsInChildren<LevelTargetLimbPart>().ToArray();
        }

#endif

        #endregion
    }
}
