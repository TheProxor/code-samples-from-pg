using Spine.Unity;
using UnityEngine;
using Drawmasters.Helpers;
using Drawmasters.Ui;
using Drawmasters.Utils;


namespace Drawmasters.Levels
{
    public class PetSkinLink : MonoBehaviour, ICoinCollector, ICurrencyAbsorber
    {
        #region Fields

        [SerializeField] private SkeletonAnimationEffectPlayer[] animationEffectPlayers = default;

        [SerializeField] private SkeletonAnimation skeletonAnimation = default;
        [SerializeField] private MeshRenderer meshRenderer = default;

        [SerializeField] private IngameTouchMonitor ingameTouchMonitor = default;
        [SerializeField] private Collider2D mainCollider2D = default;
        [SerializeField] private PetChargePhysicsButton petChargeButton = default;


        [SerializeField] private BoneFollower fxInOutBone = default;
        [SerializeField] private BoneFollower fxMagicBone = default;

        #endregion



        #region Properties

        public PetSkinType SkinType { get; private set; }

        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        public MeshRenderer MeshRenderer => meshRenderer;
         
        public IngameTouchMonitor IngameTouchMonitor => ingameTouchMonitor;

        public PetChargePhysicsButton PetChargeButton => petChargeButton;

        public Collider2D MainCollider2D => mainCollider2D;

        public Transform FxInOut => fxInOutBone.transform;

        public Transform FxMagic => fxMagicBone.transform;

        public SkeletonAnimationEffectPlayer[] AnimationEffectPlayers => animationEffectPlayers;

        #endregion



        #region Methods

        public void Initialize(PetSkinType skinType, ShooterColorType colorType)
        {
            SkinType = skinType;

            PetSkinsSettings settings = IngameData.Settings.pets.skinsSettings;
            string skinName = settings.GetAssetSkin(skinType, colorType);

            SkeletonAnimation.AnimationState.ClearTracks();
            SkeletonAnimation.skeletonDataAsset = settings.GetSkeletonDataAsset(skinType);
            SkeletonAnimation.initialSkinName = string.Empty;


            SkeletonAnimation.Initialize(true);
            SkeletonAnimation.skeleton.SetSkin(skinName);
            SkeletonAnimation.skeleton.SetToSetupPose();


            PetAnimationSettings animSettings = IngameData.Settings.pets.animationSettings;
            fxInOutBone.skeletonRenderer = SkeletonAnimation;
            fxInOutBone.SetBone(animSettings.FxInOutBoneName(skinType));

            fxMagicBone.skeletonRenderer = SkeletonAnimation;
            fxMagicBone.SetBone(animSettings.FxMagicBoneName(skinType));

            petChargeButton.Initialize();
        }
        

        public void Deinitialize() =>
            petChargeButton.Deinitialize();

        #endregion



        #region ICoinCollector

        public Vector2 CurrentPosition
        {
            get => transform.position;
            set => transform.position = value;
        }

        #endregion



        #region ICurrencyAbsorber

        public Transform TargetAbsorbTransform =>
            transform;

        #endregion



        #region Editor

#if UNITY_EDITOR

        [Sirenix.OdinInspector.Button]
        private void AssignValues()
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            gameObject.SetLayerRecursively(LayerMask.NameToLayer(PhysicsLayers.Pet));
            petChargeButton = GetComponentInChildren<PetChargePhysicsButton>();
        }
        
#endif

        #endregion
    }
}