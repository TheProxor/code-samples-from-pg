using Core;
using Drawmasters.LevelsRepository;
using Drawmasters.LevelsRepository.Editor;
using System;
using System.Collections.Generic;
using Drawmasters.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class LevelEditor : MonoBehaviour
    {
        #region Nested types

        public enum State
        {
            None,

            LevelObservation,
            ObjectsTransformation,
            GameCameraMovement
        }

        #endregion



        #region Fields

        public static event Action<LevelHeader> OnPlayRequest;
        public static event Action<LevelHeader> OnReturnRequest;
        public static event Action OnShouldChangeCameraBorders;

        [SerializeField] private EditorLevel level = default;
        [SerializeField] private Button openLibraryButton = default;
        [SerializeField] private LibraryMenu libraryMenu = default;
        [SerializeField] private Button openObjectInspectorButton = default;
        [SerializeField] private SelectedObjectInspector objectInspector = default;
        [SerializeField] private GameObject buttonsRoot = default;
        [SerializeField] private Button returnButton = default;
        [SerializeField] private Button playButton = default;
        [SerializeField] private Button saveButton = default;
        [SerializeField] private Button previousButton = default;
        [SerializeField] private Button nextButton = default;
        [SerializeField] private Button changeCameraBordersButton = default;
        [SerializeField] private TextMeshProUGUI changePhysicsActivityLabel = default;
        [SerializeField] private Button changePhysicsActivityButton = default;
        [SerializeField] private Button changeCollisionActivityButton = default;
        [SerializeField] private KeyCode changeCollisionActivityKey = default;
        [SerializeField] private TextMeshProUGUI changeCollisionActivityLabel = default;
        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private SelectedObjectHandle selectedObjectHandle = default;
        [SerializeField] private BossLevelInspector bossLevelInspector = default;
        [SerializeField] private FloatInputUi pathInput = default;
        [SerializeField] private BonusLevelInspector bonusLevelInspector = default;

        static State currentState = State.None;

        private Action<Vector3> onFinishedCameraPositionChange;
        private LevelHeader currentHeader;

        #endregion



        #region Properties

        public static State CurrentState
        {
            get => currentState;
            private set
            {
                if (CurrentState != value)
                {
                    currentState = value;
                    ChangeState.Register(CurrentState);
                }
            }
        }

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            SelectedObjectChange.Subscribe(OnSelectedObjectsChange);
            StartCameraPositionChange.Subscribe(OnStartCameraPositionChange);

            openLibraryButton.onClick.AddListener(libraryMenu.Open);
            openObjectInspectorButton.onClick.AddListener(() =>
            {
                openObjectInspectorButton.gameObject.SetActive(false);
                objectInspector.Open();
            });
            objectInspector.OnClosed += () => { openObjectInspectorButton.gameObject.SetActive(true); };
            returnButton.onClick.AddListener(() =>
            {
                if (CurrentState == State.GameCameraMovement)
                {
                    OnFinishCameraPositionChange();
                }
                else
                {
                    OnReturnRequest?.Invoke(currentHeader);
                }
            });

            playButton.onClick.AddListener(PlayButton_OnClick);
            saveButton.onClick.AddListener(SaveButton_OnClick);
            nextButton.onClick.AddListener(NextButton_OnClick);
            previousButton.onClick.AddListener(PreviousButton_OnClick);
            changeCameraBordersButton.onClick.AddListener(ChangeCameraBordersButton_OnClick);

            changePhysicsActivityButton.onClick.AddListener(ChangePhysicsActivityButton_OnClick);
            changeCollisionActivityButton.onClick.AddListener(ChangeCollisionActivityButton_OnClick);
            
            pathInput.OnValueChange += PathInput_OnValueChange;

            InputKeys.EventInputKeyDown.Subscribe(changeCollisionActivityKey, ChangeCollisionActivityButton_OnClick);
            EnemyBossExtensionRocketDraw.OnShouldSetSelectionLock += EnemyBossExtensionRocketDraw_OnShouldSetSelectionLock;

            EventSystemController.SetupEventSystem(EventSystem.current);
            EventSystemController.EnableEventSystem();

            SetButtonsActive(true);
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (CurrentState)
                {
                    case State.GameCameraMovement:
                        OnFinishCameraPositionChange();
                        break;

                    case State.LevelObservation:
                        OnReturnRequest?.Invoke(currentHeader);
                        break;
                }
            }
        }


        private void OnDestroy()
        {
            SelectedObjectChange.Unsubscribe(OnSelectedObjectsChange);
            StartCameraPositionChange.Unsubscribe(OnStartCameraPositionChange);

            playButton.onClick.RemoveListener(NextButton_OnClick);
            saveButton.onClick.RemoveListener(SaveButton_OnClick);
            nextButton.onClick.RemoveListener(NextButton_OnClick);
            previousButton.onClick.RemoveListener(PreviousButton_OnClick);
            changeCameraBordersButton.onClick.RemoveListener(ChangeCameraBordersButton_OnClick);

            pathInput.OnValueChange -= PathInput_OnValueChange;

            changePhysicsActivityButton.onClick.RemoveListener(ChangePhysicsActivityButton_OnClick);
            changeCollisionActivityButton.onClick.RemoveListener(ChangeCollisionActivityButton_OnClick);
            InputKeys.EventInputKeyDown.Subscribe(changeCollisionActivityKey, ChangeCollisionActivityButton_OnClick);
            EnemyBossExtensionRocketDraw.OnShouldSetSelectionLock -= EnemyBossExtensionRocketDraw_OnShouldSetSelectionLock;
        }

        #endregion



        #region Methods

        public void Init(LevelHeader header) => LoadLevel(header);


        private void SetButtonsActive(bool areActive) => buttonsRoot.gameObject.SetActive(areActive);


        private void LoadLevel(LevelHeader header)
        {
            currentHeader = header;
            objectInspector.SetupHeader(currentHeader);
            libraryMenu.SetupHeader(currentHeader);
            level.Load(header);
            CurrentState = State.LevelObservation;

            titleLabel.text = "Title: " + header.title;

            previousButton.gameObject.SetActive(CurrentHeaderIndex() != 0);
            nextButton.gameObject.SetActive(ConstructorLevelsManager.GetLevelHeaders(header.mode).Length - 1 != CurrentHeaderIndex());

            bossLevelInspector.gameObject.SetActive(header.levelType == LevelType.Boss);
            selectedObjectHandle.Deselect();

            bossLevelInspector.Deinitialize();
            bossLevelInspector.Initialize(level, header.stagesCount);
            
            bonusLevelInspector.gameObject.SetActive(header.levelType == LevelType.Bonus);
            bonusLevelInspector.Deinitialize();
            bonusLevelInspector.Initialize(level, 3);
            
            pathInput.Init("Path", ConstructorLevelsManager.GetLevelData(header).pathDistance, 0f, float.MaxValue);
        }

        #endregion



        #region Events handlers
        
        private void PathInput_OnValueChange(float pathValue)
        {
            Level.Data data = ConstructorLevelsManager.GetLevelData(currentHeader);

            data.pathDistance = pathValue;
            
            level.Save();
        }

        private void OnSelectedObjectsChange(List<EditorLevelObject> selectedObjects)
        {
            if ((currentState == State.LevelObservation) || (currentState == State.ObjectsTransformation))
            {
                CurrentState = ((selectedObjects.Count == 0) ? (State.LevelObservation) : (State.ObjectsTransformation));
            }
        }


        private void OnStartCameraPositionChange(Vector3 cameraPosition, Action<Vector3> onFinished)
        {
            CurrentState = State.GameCameraMovement;

            level.GamePreviewCamera.Position = cameraPosition;
            level.GamePreviewCamera.Init();
            onFinishedCameraPositionChange = onFinished;
        }


        private void OnFinishCameraPositionChange()
        {
            CurrentState = State.LevelObservation;

            onFinishedCameraPositionChange?.Invoke(level.GamePreviewCamera.Position);
            onFinishedCameraPositionChange = null;
        }


        private void PlayButton_OnClick() =>
            OnPlayRequest?.Invoke(currentHeader);


        private void SaveButton_OnClick()
        {
            level.Save();

            if (currentHeader.levelType == LevelType.Boss)
            {
                level.SaveStage(BossLevelInspector.CurrentStage);
            }
        }


        private void NextButton_OnClick()
        {
            LevelHeader headerToLoad = ConstructorLevelsManager.GetLevelHeaders(currentHeader.mode)[CurrentHeaderIndex() + 1];
            LoadLevel(headerToLoad);
        }


        private void PreviousButton_OnClick()
        {
            LevelHeader headerToLoad = ConstructorLevelsManager.GetLevelHeaders(currentHeader.mode)[CurrentHeaderIndex() - 1];
            LoadLevel(headerToLoad);
        }


        private int CurrentHeaderIndex()
        {
            LevelHeader[] headers = ConstructorLevelsManager.GetLevelHeaders(currentHeader.mode);
            return Array.IndexOf(headers, currentHeader);
        }


        private void ChangePhysicsActivityButton_OnClick()
        {
            level.SetPhysicsEnabled(!level.IsPhysicsEnabled);
            changePhysicsActivityLabel.text = (level.IsPhysicsEnabled) ? ("Disable Physics") : ("Enable Physics");
        }


        private void ChangeCollisionActivityButton_OnClick()
        {
            level.SetCollisionEnabled(!level.IsCollisionEnabled);
            changeCollisionActivityLabel.text = (level.IsCollisionEnabled) ? ("Disable Collision") : ("Enable Collision");
        }


        private void ChangeCameraBordersButton_OnClick()
        {
            OnShouldChangeCameraBorders?.Invoke();
        }


        private void EnemyBossExtensionRocketDraw_OnShouldSetSelectionLock(bool enabled)
        {
            selectedObjectHandle.SetSelectionLocked(enabled);
        }

        #endregion
    }
}
