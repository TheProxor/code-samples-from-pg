using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Proposal;


namespace Drawmasters.LevelConstructor
{
    public class SelectedObjectInspector : MenuBase
    {
        #region Nested types

        enum SelectionType
        {
            None,
            Single,
            Multiple
        }

        #endregion



        #region Variables

        const float MaxPositionDifference = 0.01f;

        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private Button lockToggle = default;
        [SerializeField] private Image lockToggleImage = default;
        [SerializeField] private Sprite freeSprite = default;
        [SerializeField] private Sprite lockedSprite = default;
        [SerializeField] private Vector3UI positionField = default;
        [SerializeField] private PointsInputHandler pointsInputHandler = default;
        [SerializeField] private Float3InputUi multipleObjectsPositionInput = default;
        [SerializeField] private Vector3UI rotationField = default;
        [SerializeField] private BoolInputUi lockZToggle = default;
        [SerializeField] private LinkerInterface linkerInterface = default;
        [Space]
        [SerializeField] private MoveInputHandler moveInputHandler = default;
        [Space]
        [SerializeField] private Transform inspectorsRoot = default;


        [Header("Boss settings")]
        [SerializeField] private FloatInputUi fallVelocityInputUi = default;
        [SerializeField] private BoolInputUi isSlowmotionOnPush = default;
        [SerializeField] private BoolInputUi isFreeFallToggle = default;
        [SerializeField] private GameObject bossLevelSeparator = default;

        [Header("Bonus level parameters")] 
        [SerializeField] private FloatInputUi bonusLevelStageFloatInput = default;
        [SerializeField] private Vector3UI bonusLevelVelocity = default;
        [SerializeField] private FloatInputUi bonusLevelAcceleration = default;
        [SerializeField] private FloatInputUi bonusLevelAngularVelocity = default;
        [SerializeField] private SliderInputUi bonusLevelRewardTypeSlider = default;
        [SerializeField] private SliderInputUi bonusLevelCurrencySlider = default;
        [SerializeField] private FloatInputUi bonusLevelRewardAmount = default;
        [SerializeField] private SliderInputUi bonusLevelPetSkinTypeSlider = default;
        [SerializeField] private GameObject bonusLevelSeparator = default;

        private EditorLevelObject selectedObject = null;
        private List<EditorLevelObject> selectedObjects = new List<EditorLevelObject>();

        private List<GameObject> additionalInspectors = new List<GameObject>();

        private SelectionType currentSelectionType = SelectionType.None;

        private LevelsRepository.LevelHeader header;

        #endregion



        #region Properties

        private SelectionType CurrentSelectionType
        {
            get => currentSelectionType;
            set
            {
                currentSelectionType = value;

                switch (value)
                {
                    case SelectionType.Single:
                        selectedObjects = null;
                        break;
                    case SelectionType.Multiple:
                        selectedObject = null;
                        break;
                    case SelectionType.None:
                        selectedObjects = null;
                        selectedObject = null;
                        break;
                }
            }
        }

        #endregion



        #region Unity Lifecycle

        private void Awake()
        {
            positionField.OnValueChange += PositionField_OnValueChange;

            multipleObjectsPositionInput.OnChangeX += MultipleObjectsPositionInput_OnChangeX;
            multipleObjectsPositionInput.OnChangeY += MultipleObjectsPositionInput_OnChangeY;
            multipleObjectsPositionInput.OnChangeZ += MultipleObjectsPositionInput_OnChangeZ;

            rotationField.OnValueChange += RotationField_OnValueChange;

            lockToggle.onClick.AddListener(() =>
            {
                selectedObject.IsLocked = !selectedObject.IsLocked;
                UpdateLockSprite();
            });

            lockZToggle.OnValueChange += (isLockZ) =>
            {
                selectedObject.IsZLocked = isLockZ;

                Vector3 buttonsVisibility = isLockZ ? new Vector3(1.0f, 1.0f, 0.0f) : Vector3.one;
                positionField.UpdateButtonsVisibility(buttonsVisibility);
                multipleObjectsPositionInput.UpdateButtonsVisibility(buttonsVisibility);
            };

            fallVelocityInputUi.OnValueChange += (value) => selectedObject.ComeVelocity = value;
            isSlowmotionOnPush.OnValueChange += (value) => selectedObject.IsSlowmotionOnPush = value;

            isFreeFallToggle.OnValueChange += (value) => selectedObject.IsNextStageFreeFall = value;

            bonusLevelStageFloatInput.OnValueChange +=
                stageIndex => selectedObject.BonusLevelStageIndex = (int) stageIndex;
            
            bonusLevelVelocity.OnValueChange += velocity => selectedObject.BonusLevelVelocity = velocity.ToVector2();

            bonusLevelAcceleration.OnValueChange +=
                acceleration => selectedObject.BonusLevelAcceleration = acceleration;

            bonusLevelAngularVelocity.OnValueChange += angular => selectedObject.BonusLevelAngularVelocity = angular;

            bonusLevelRewardTypeSlider.OnValueChange += value =>
            {
                int number = (int)value;

                RewardType type = (RewardType)number;

                selectedObject.RewardType = type;

                bonusLevelRewardTypeSlider.SetTitle($"Reward: {type}");

                CommonUtility.SetObjectActive(bonusLevelCurrencySlider.gameObject, type == RewardType.Currency);
                CommonUtility.SetObjectActive(bonusLevelRewardAmount.gameObject, type == RewardType.Currency);
                CommonUtility.SetObjectActive(bonusLevelPetSkinTypeSlider.gameObject, type == RewardType.PetSkin);
            };

            bonusLevelCurrencySlider.OnValueChange += value =>
            {
                int number = (int) value;

                CurrencyType type = (CurrencyType) number;

                selectedObject.RewardCurrencyType = type;
                
                bonusLevelCurrencySlider.SetTitle($"Currency: {type}");

                bonusLevelRewardAmount.gameObject.SetActive(type != CurrencyType.None);
            };

            bonusLevelPetSkinTypeSlider.OnValueChange += value =>
            {
                int number = (int)value;

                PetSkinType type = (PetSkinType)number;

                selectedObject.PetSkinType = type;

                bonusLevelPetSkinTypeSlider.SetTitle($"Pet: {type}");
            };

            bonusLevelRewardAmount.OnValueChange += amount => selectedObject.RewardCurrencyAmount = amount;
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            SelectedObjectChange.Subscribe(OnSelectedObjectChange);
            EditorLevelStageChange.Subscribe(OnStageChange);
        }


        protected override void OnDisable()
        {
            base.OnDisable();

            SelectedObjectChange.Unsubscribe(OnSelectedObjectChange);
            EditorLevelStageChange.Unsubscribe(OnStageChange);
        }


        private void Start()
        {
            positionField.Init("Position", 2);
            rotationField.Init("Rotation", 2);
            multipleObjectsPositionInput.Init("Position", 2);

            UpdateInterface();
        }


        private void Update()
        {
            switch (CurrentSelectionType)
            {
                case SelectionType.Single:
                    positionField.SetCurrentValue(selectedObject.transform.position);
                    rotationField.SetCurrentValue(selectedObject.transform.eulerAngles);
                    break;
                case SelectionType.Multiple:
                    UpdateObjectsPositionUi();
                    break;
            }
        }

        #endregion



        #region Methods

        public void SetupHeader(LevelsRepository.LevelHeader _header) =>
            header = _header;
        

        private void UpdateInterface()
        {
            foreach (var additionalInspector in additionalInspectors)
            {
                Destroy(additionalInspector.gameObject);
            }

            additionalInspectors.Clear();
            lockToggle.gameObject.SetActive(CurrentSelectionType == SelectionType.Single);
            multipleObjectsPositionInput.gameObject.SetActive(CurrentSelectionType == SelectionType.Multiple);
            lockZToggle.gameObject.SetActive(CurrentSelectionType == SelectionType.Single);
             linkerInterface.gameObject.SetActive((CurrentSelectionType == SelectionType.Single) && selectedObject.CanHaveLinks);

            positionField.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && selectedObject.AvailableHandlers.Contains(HandlersControllerType.Axis));
            rotationField.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && selectedObject.AvailableHandlers.Contains(HandlersControllerType.Rotation));

            if (header != null)
            {
                // boss panel
                isSlowmotionOnPush.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Boss);
                fallVelocityInputUi.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Boss);
                isFreeFallToggle.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Boss);
                bossLevelSeparator.gameObject.SetActive(header.levelType == LevelType.Boss);

                // bonus panel
                bonusLevelStageFloatInput.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Bonus);
                bonusLevelVelocity.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Bonus);
                bonusLevelAcceleration.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Bonus);
                bonusLevelAngularVelocity.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Bonus);
                bonusLevelRewardTypeSlider.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Bonus);
                bonusLevelPetSkinTypeSlider.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Bonus);
                bonusLevelCurrencySlider.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Bonus);
                bonusLevelRewardAmount.gameObject.SetActive(CurrentSelectionType == SelectionType.Single && header.levelType == LevelType.Bonus && selectedObject.RewardCurrencyType != CurrencyType.None);
                bonusLevelSeparator.gameObject.SetActive(header.levelType == LevelType.Bonus);
            }

            switch (CurrentSelectionType)
            {
                case SelectionType.Single:

                    UpdateLockSprite();
                    titleLabel.text = selectedObject.name.Replace("(Clone)", string.Empty);
                    lockZToggle.Init("Lock Z", selectedObject.IsZLocked);
                    isSlowmotionOnPush.Init("Slowmo push", selectedObject.IsSlowmotionOnPush);
                    fallVelocityInputUi.Init("Come velocity", selectedObject.ComeVelocity, 0.0f);
                    isFreeFallToggle.Init("Next Fall", selectedObject.IsNextStageFreeFall);
                    
                    bonusLevelStageFloatInput.Init("Bonus level stage", selectedObject.BonusLevelStageIndex, 0, Byte.MaxValue);
                    bonusLevelVelocity.Init("Start velocity", 2);
                    bonusLevelVelocity.SetCurrentValue(selectedObject.BonusLevelVelocity);
                    bonusLevelAcceleration.Init("Acceleration", selectedObject.BonusLevelAcceleration);
                    bonusLevelAngularVelocity.Init("Angular velocity", selectedObject.BonusLevelAngularVelocity);

                    RewardType currentRewardType = selectedObject.RewardType;
                    int currentRewardTypeSliderValue = (int)currentRewardType;
                    bonusLevelRewardTypeSlider.Init($"Reward: {selectedObject.RewardType}", currentRewardTypeSliderValue, 0, Enum.GetValues(typeof(RewardType)).Length - 1);
                    bonusLevelRewardTypeSlider.MarkWholeNumbersOnly();

                    PetSkinType currentPetSkinType = selectedObject.PetSkinType;
                    int currentPetSkinSliderValue = (int)currentPetSkinType;
                    bonusLevelPetSkinTypeSlider.Init($"Pet: {selectedObject.PetSkinType}", currentPetSkinSliderValue, 0, Enum.GetValues(typeof(PetSkinType)).Length - 1);
                    bonusLevelPetSkinTypeSlider.MarkWholeNumbersOnly();

                    CurrencyType currentType = selectedObject.RewardCurrencyType;
                    int currentSliderValue = (int)currentType;
                    bonusLevelCurrencySlider.Init($"Currency: {selectedObject.RewardCurrencyType}", currentSliderValue, 0, Enum.GetValues(typeof(CurrencyType)).Length - 1);
                    bonusLevelCurrencySlider.MarkWholeNumbersOnly();

                    bonusLevelRewardAmount.Init("Reward amount", selectedObject.RewardCurrencyAmount, 0f);

                    CommonUtility.SetObjectActive(bonusLevelRewardAmount.gameObject, currentRewardType == RewardType.Currency);
                    CommonUtility.SetObjectActive(bonusLevelCurrencySlider.gameObject, currentRewardType == RewardType.Currency && selectedObject.RewardCurrencyAmount > 0);
                    CommonUtility.SetObjectActive(bonusLevelPetSkinTypeSlider.gameObject, currentRewardType == RewardType.PetSkin);

                    if (selectedObject.AvailableHandlers.Contains(HandlersControllerType.Points))
                    {
                        pointsInputHandler.gameObject.SetActive(true);
                        pointsInputHandler.Initialize(selectedObject);
                    }

                    if (linkerInterface.gameObject.activeSelf)
                    {
                        linkerInterface.Init(selectedObject);
                    }

                    CommonUtility.SetObjectActive(moveInputHandler.gameObject, true);
                    moveInputHandler.Initialize(selectedObject);

                    InspectorExtensionBase inspectorExtension = selectedObject.GetAdditionalInspector();
                    if (inspectorExtension != null)
                    {
                        InspectorExtensionBase inspector = Instantiate(inspectorExtension, inspectorsRoot);
                        inspector.Init(selectedObject);
                        additionalInspectors.Add(inspector.gameObject);
                    }


                    break;
                case SelectionType.Multiple:

                    titleLabel.text = $"{selectedObjects.Count} Objects";
                    pointsInputHandler.gameObject.SetActive(false);
                    CommonUtility.SetObjectActive(moveInputHandler.gameObject, false);

                    break;

                case SelectionType.None:

                    pointsInputHandler.gameObject.SetActive(false);
                    CommonUtility.SetObjectActive(moveInputHandler.gameObject, false);
                    titleLabel.text = "Select Object";

                    break;
            }
        }


        private void UpdateLockSprite() => lockToggleImage.sprite = (selectedObject.IsLocked ? lockedSprite : freeSprite);


        private void UpdateObjectsPositionUi()
        {
            if (selectedObjects.Count > 0)
            {
                float? x = selectedObjects[0].transform.position.x;
                float? y = selectedObjects[0].transform.position.y;
                float? z = selectedObjects[0].transform.position.z;

                for (int i = 1; i < selectedObjects.Count; i++)
                {
                    Vector3 position = selectedObjects[i].transform.position;

                    if (x.HasValue && (MaxPositionDifference < Mathf.Abs(x.Value - position.x)))
                    {
                        x = null;
                    }

                    if (y.HasValue && (MaxPositionDifference < Mathf.Abs(y.Value - position.y)))
                    {
                        y = null;
                    }

                    if (z.HasValue && (MaxPositionDifference < Mathf.Abs(z.Value - position.z)))
                    {
                        z = null;
                    }
                }

                multipleObjectsPositionInput.SetCurrentValue(x, y, z);
            }
        }

        #endregion



        #region Events Handlers

        private void OnSelectedObjectChange(List<EditorLevelObject> newSelectedObjects)
        {
            if (newSelectedObjects.Count == 0)
            {
                CurrentSelectionType = SelectionType.None;
            }
            else if (newSelectedObjects.Count == 1)
            {
                CurrentSelectionType = SelectionType.Single;
                selectedObject = newSelectedObjects[0];
            }
            else
            {
                CurrentSelectionType = SelectionType.Multiple;
                selectedObjects = new List<EditorLevelObject>(newSelectedObjects);
            }

            UpdateInterface();
        }

        private void PositionField_OnValueChange(Vector3 position)
        {
            if (selectedObject != null)
            {
                selectedObject.transform.position = position;
            }
        }


        private void MultipleObjectsPositionInput_OnChangeX(float x)
        {
            foreach (var selectedObject in selectedObjects)
            {
                selectedObject.Move(selectedObject.transform.position.SetX(x));
            }

            UpdateObjectsPositionUi();
        }


        private void MultipleObjectsPositionInput_OnChangeY(float y)
        {
            foreach (var selectedObject in selectedObjects)
            {
                selectedObject.Move(selectedObject.transform.position.SetY(y));
            }

            UpdateObjectsPositionUi();
        }


        private void MultipleObjectsPositionInput_OnChangeZ(float z)
        {
            foreach (var selectedObject in selectedObjects)
            {
                selectedObject.Move(selectedObject.transform.position.SetZ(z));
            }

            UpdateObjectsPositionUi();
        }


        private void RotationField_OnValueChange(Vector3 rotation)
        {
            if (selectedObject != null)
            {
                selectedObject.transform.eulerAngles = rotation;
            }
        }


        private void OnStageChange(int stage) =>
            UpdateInterface();

        #endregion
    }
}
