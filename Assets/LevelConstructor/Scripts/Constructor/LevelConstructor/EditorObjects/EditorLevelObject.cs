using Drawmasters.Editor.Utils;
using Drawmasters.Levels;
using Drawmasters.Proposal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorLevelObject : MonoBehaviour
    {
        #region Fields

        const string LockedObjectPath = "Assets/LevelConstructor/Prefabs/Constructor/LevelEditor/Lock.prefab";
        const string SelectedMaterialPath = "Assets/LevelConstructor/Materials/SelectedObject.mat";
        const string InspectorExtensionsFolder = "Assets/LevelConstructor/Prefabs/Constructor/LevelEditor/InspectorExtensions";

        [SerializeField] private bool isStaticByDefault = default;
        [SerializeField] private int refObjectIndex = default;
        [SerializeField] private bool isZLocked = true;
        [SerializeField] private bool canHaveMultipleLinkedObjects = false;

        [SerializeField]
        List<GameMode> availableGameModes =
            new List<GameMode>(Enum.GetValues(typeof(GameMode)).Cast<GameMode>().ToList());
        [SerializeField]
        List<HandlersControllerType> availableHandlers = new List<HandlersControllerType>()
        {
            HandlersControllerType.Axis,
            HandlersControllerType.Rotation
        };

        private static GameObject lockedIconPrefab;
        private static Material selectedMaterial;

        private bool isLocked;
        protected bool isSelected;

        private GameObject lockedIcon;

        private Renderer[] renderers;

        private EditorAsset<InspectorExtensionBase> asset;

        private RigidbodyConstraints2D defaultConstraints;
        private float defaultGravityScale;

        private LevelObjectData currentData;

        #endregion



        #region Properties

        public int Index
        {
            get => refObjectIndex;
            set => refObjectIndex = value;
        }


        public bool IsStaticByDefault
        {
            get => isStaticByDefault;
            set => isStaticByDefault = value;
        }


        public bool IsZLocked
        {
            get => isZLocked;
            set => isZLocked = value;
        }


        public Rigidbody2D Rigidbody2D { get; protected set; }


        public bool CanHaveLinks => true;


        public bool CanHaveMultipleLinkedObjects
        {
            get => canHaveMultipleLinkedObjects;
            set => canHaveMultipleLinkedObjects = value;
        }


        public bool IsLocked
        {
            get => isLocked;
            set
            {
                isLocked = value;

                lockedIcon.SetActive(value);

                if (value)
                {
                    SetSelectedObjectPhysics(false);
                }
                else
                {
                    SetSelectedObjectPhysics(isSelected);
                }
            }
        }


        public LevelObjectMoveSettings MoveSettings { get; private set; } = new LevelObjectMoveSettings();

        public List<Vector3> JointsPoints { get; set; } = new List<Vector3>();

        public float ComeVelocity { get; set; } = 200.0f;

        public bool IsSlowmotionOnPush { get; set; }

        public float FallDelay { get; set; }

        public bool IsNextStageFreeFall { get; set; }

        public int CreatedStageIndex { get; private set; }
        
        

        public virtual Vector3 Center => transform.position;

        public List<HandlersControllerType> AvailableHandlers => availableHandlers;

        #endregion
        
        
        
        #region Bonus level properties
        
        public int BonusLevelStageIndex { get; set; }
        
        public Vector2 BonusLevelVelocity { get; set; }
        
        
        public float BonusLevelAcceleration { get; set; }
        
        
        public float BonusLevelAngularVelocity { get; set; }

        public RewardType RewardType { get; set; }


        public CurrencyType RewardCurrencyType { get; set; }
        
        public float RewardCurrencyAmount { get; set; }


        public PetSkinType PetSkinType { get; set; }

        #endregion



        #region Unity lifecycle

        protected virtual void Awake()
        {
            GetRigidBody();

            if (lockedIconPrefab == null)
            {
                lockedIconPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LockedObjectPath);
            }

            if (selectedMaterial == null)
            {
                selectedMaterial = AssetDatabase.LoadAssetAtPath<Material>(SelectedMaterialPath);
            }

            RefreshRenderers();

            lockedIcon = Instantiate(lockedIconPrefab, transform);
            lockedIcon.transform.localPosition = Vector3.zero;
            lockedIcon.SetActive(false);
        }

        #endregion



        #region Methods

        public virtual LevelObjectData GetData()
        {
            LevelObjectData data = new LevelObjectData
            {
                index = Index,
                position = transform.position,
                rotation = transform.eulerAngles,
                isLockZ = IsZLocked,
                moveSettings = MoveSettings,
                jointsPoints = JointsPoints,
                createdStageIndex = CreatedStageIndex,
                shouldPlayEffectsOnPush = IsSlowmotionOnPush,
                
                bonusData = new BonusLevelObjectData()
                {                  
                    angularVelocity = BonusLevelAngularVelocity,
                    linearVelocity = BonusLevelVelocity,
                    stageIndex = BonusLevelStageIndex,
                    acceleration = BonusLevelAcceleration,
                    rewardType = RewardType,
                    currencyType = RewardCurrencyType,
                    currencyAmount = RewardCurrencyAmount,
                    petSkinType = PetSkinType
                }
            };

            LoadDefaultData();

            return data;
        }


        public virtual void SetData(LevelObjectData data)
        {
            transform.position = data.position;
            transform.eulerAngles = data.rotation;
            MoveSettings = data.moveSettings;
            JointsPoints = data.jointsPoints;
            CreatedStageIndex = data.createdStageIndex;
            IsSlowmotionOnPush = data.shouldPlayEffectsOnPush;

            BonusLevelStageIndex = data.bonusData.stageIndex;
            BonusLevelAcceleration = data.bonusData.acceleration;
            BonusLevelVelocity = data.bonusData.linearVelocity;
            BonusLevelAngularVelocity = data.bonusData.angularVelocity;
            RewardType = data.bonusData.rewardType;
            
            RewardCurrencyType = data.bonusData.currencyType;
            RewardCurrencyAmount = data.bonusData.currencyAmount;

            PetSkinType = data.bonusData.petSkinType;


            IsZLocked = data.isLockZ;

            currentData = data;
        }


        public void SetStageData(StageLevelObjectData data)
        {
            transform.position = data.position;
            transform.localEulerAngles = data.rotation;
            JointsPoints = data.jointsPoints;

            ComeVelocity = data.comeVelocity;
        }


        public void TryAddRigidbody()
        {
            if (Rigidbody2D == null)
            {
                Rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
                Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                defaultConstraints = Rigidbody2D.constraints;
            }
        }


        public virtual void AddLink(EditorLevelObject linkedObject)
        {
            EditorLinker.AddLink(this, linkedObject);
        }


        public virtual void RemoveLink(EditorLevelObject linkedObject)
        {
            EditorLinker.RemoveLink(this, linkedObject);
        }


        public virtual InspectorExtensionBase GetAdditionalInspector() => null;


        public bool IsLevelObjectAvailableForGameMode(GameMode gameMode) => availableGameModes.Contains(gameMode);


        public void Select()
        {
            isSelected = true;

            if (!IsLocked)
            {
                SetSelectedObjectPhysics(true);
            }

            foreach (var renderer in renderers)
            {
                Material[] materials = renderer.materials;
                Array.Resize(ref materials, renderer.materials.Length + 1);
                materials[materials.Length - 1] = selectedMaterial;
                renderer.materials = materials;
            }
        }


        public void Deselect()
        {
            isSelected = false;

            if (!IsLocked)
            {
                SetSelectedObjectPhysics(false);
            }

            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    Material[] materials = renderer.materials;
                    Array.Resize(ref materials, renderer.materials.Length - 1);
                    renderer.materials = materials;
                }
            }
        }


        public void StartRotating()
        {
            Rigidbody2D.constraints = RigidbodyConstraints2D.FreezePosition;
        }


        public void FinishRotating()
        {
            Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }


        public virtual void Rotate(Vector3 newRotation)
        {
            if (!IsLocked)
            {
                Rigidbody2D.MoveRotation(newRotation.z);
                transform.Rotate(newRotation);
            }
        }


        public virtual void Move(Vector3 newPosition)
        {
            if (!IsLocked)
            {
                Rigidbody2D.MovePosition(newPosition);
            }
        }


        public void SetPhysicsEnabled(bool shouldEnable)
        {
            if (Rigidbody2D != null && !isSelected)
            {
                Rigidbody2D.isKinematic = !shouldEnable || isStaticByDefault;
                Rigidbody2D.gravityScale = (shouldEnable && !isStaticByDefault) ? defaultGravityScale : default;
                Rigidbody2D.velocity = Rigidbody2D.isKinematic ? Vector2.zero : Rigidbody2D.velocity;
            }
        }


        public bool IsDataEqual(LevelObjectData data) => data == currentData;


        protected void RefreshRenderers() =>
            renderers = GetComponentsInChildren<Renderer>(true);

        protected InspectorExtensionBase GetAdditionalInspector(string inspectorFileName)
        {
            if (asset == null)
            {
                asset = new EditorAsset<InspectorExtensionBase>($"{InspectorExtensionsFolder}/{inspectorFileName}");
            }

            return asset.Value;
        }


        protected virtual void GetRigidBody()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();

            if (Rigidbody2D != null)
            {
                defaultConstraints = Rigidbody2D.constraints;
                defaultGravityScale = Rigidbody2D.gravityScale;
            }
        }


        protected virtual void LoadDefaultData() { }


        private void SetSelectedObjectPhysics(bool isSelected)
        {
            if (Rigidbody2D != null)
            {
                if (isSelected)
                {
                    Rigidbody2D.isKinematic = false;
                    Rigidbody2D.gravityScale = default;
                    Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
                }
                else
                {
                    SetPhysicsEnabled(false);
                    Rigidbody2D.constraints = defaultConstraints;
                }
            }
        }

        #endregion
    }
}
