using UnityEngine;
using System;
using System.Linq;
using Drawmasters.Levels;
using Drawmasters.Proposal;


namespace Drawmasters.Pets
{
    [CreateAssetMenu(fileName = nameof(PetLevelSettings),
        menuName = NamingUtility.MenuItems.IngameSettings + nameof(PetLevelSettings))]
    public class PetLevelSettings : ScriptableObjectData<PetLevelSettings.Data, PetSkinType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<PetSkinType>
        {
            public Vector3 rectOffset = default;
            public float rectWidth = default;
            public float rectHeight = default;
            public Gradient aimGradient = default;
        }

        [Serializable]
        public class MoveTypeData
        {
            public MoveType moveType = default;
            public float velocityThreshold = default;
        }

        public enum MoveType
        {
            Slow = 0,
            Fast = 1
        }

        #endregion



        #region Fields

        [Header("Invoke")]
        public float maxChargePoints = default;
        public VectorAnimation invokeAnimation = default;

        [Header("Input")]
        public float stopDistance = default;
        public float minDistanceBetweenPoints = default;

        [Header("Move")]
        public AnimationCurve moveAnimationCurve = default;
        public float maxMoveVelocityDuration = default;

        public float baseMoveVelocity = default;
        public float maxMoveVelocity = default;

        public float lockScaleOnAttackDuration = default;

        [SerializeField] private MoveTypeData[] moveTypeData = default;

        [Header("Move (Slow Up)")]
        public AnimationCurve slowUpAnimationCurve = default;
        public float slowUpDuration = default;

        [Header("Border")]
        public float borderAdditionalOffset = default;

        [Header("Shoot")]
        public float moveStartedAttackDelay = default;
        public float attackDelay = default;

        public float attackRadius = default;
        public float delayToNextShotInLevelTarget = default;

        public int maxPetsCountOnLevel = default;
        public float projectileExplodeDelay = default;

        [Header("Aim")]
        public float aimDuration = default;

        public LineRenderer aimLineRenderer = default;
        public NumberAnimation aimLineAnimation = default;

        public GameObject aimScopePrefab = default;
        public VectorAnimation aimScopeScaleAnimation = default;

        [Header("Default Pet")]
        public PetSkinReward defaultPetSkinReward = default;
        public int levelForUnlockDefaultPet = default;

        [Header("Bonus Level")]
        public float bonusLevelAppearDelay = default;
        public float bonusLevelTeleportationDelay = default;

        public float chargePointsOnPetCollect = default;

        [Header("Ingame UI")]
        public VectorAnimation soulTrailAnimation = default;
        public FactorAnimation petButtonAlphaAnimation = default;
        
        #endregion



        #region Methods

        public Vector3 FindRectOffset(PetSkinType petSkinType)
        {
            Data foundData = FindData(petSkinType);
            return foundData == null ? default : foundData.rectOffset;
        }


        public float FindRectWidth(PetSkinType petSkinType)
        {
            Data foundData = FindData(petSkinType);
            return foundData == null ? default : foundData.rectWidth;
        }


        public float FindRectHeight(PetSkinType petSkinType)
        {
            Data foundData = FindData(petSkinType);
            return foundData == null ? default : foundData.rectHeight;
        }


        public Gradient FindAimGradient(PetSkinType petSkinType)
        {
            Data foundData = FindData(petSkinType);
            return foundData == null ? default : foundData.aimGradient;
        }
        

        public MoveType GetMoveType(float currentMoveVelocity)
        {
            var foundData = moveTypeData.OrderByDescending(e => e.velocityThreshold)
                                        .ToList()
                                        .Find(e => currentMoveVelocity >= e.velocityThreshold);
            if (foundData == null)
            {
                CustomDebug.Log($"No {nameof(MoveType)} was found for currentMoveVelocity {currentMoveVelocity}");
            }

            return foundData == null ? default : foundData.moveType;
        }


        public new Data FindData(PetSkinType petSkinType) =>
            base.FindData(petSkinType);

        #endregion

    }
}
