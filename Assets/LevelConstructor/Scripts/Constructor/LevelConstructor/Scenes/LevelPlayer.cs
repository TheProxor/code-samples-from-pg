using Drawmasters.Levels;
using Drawmasters.LevelsRepository;
using Drawmasters.Pets;
using Drawmasters.ServiceUtil;
using JoystickPlugin;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class LevelPlayer : MonoBehaviour
    {
        #region Fields

        public static event Action<LevelHeader> OnReturnRequest;

        [SerializeField] private Level level = default;        
        [SerializeField] private Button returnButton = default;
        [SerializeField] private Button replayButton = default;
        [SerializeField] private Button petInvokeButton = default;
        [SerializeField] private TextMeshProUGUI projectilesCounter = default;
        [SerializeField] private EditorLevelConsole editorLevelConsole = default;

        [SerializeField] private DynamicJoystick petMoveJoystick = default;

        private LevelHeader header;

        #endregion



        #region Properties

        public static GameMode CurrentMode { get; private set; }

        public static int CurrentIndex { get; private set; }

        #endregion



        #region Unity lifecycle

        private void OnEnable()
        {
            replayButton.onClick.AddListener(Replay);
            returnButton.onClick.AddListener(Return);
            petInvokeButton.onClick.AddListener(InvokePet);

            petMoveJoystick.SetActive(false);
            PetInvokeComponent.OnShouldInvokePetForLevel += PetInvokeComponent_OnShouldInvokePetForLevel;
        }


        private void OnDisable()
        {
            replayButton.onClick.RemoveListener(Replay);
            returnButton.onClick.RemoveListener(Return);
            petInvokeButton.onClick.RemoveListener(InvokePet);

            PetInvokeComponent.OnShouldInvokePetForLevel -= PetInvokeComponent_OnShouldInvokePetForLevel;

            if (petMoveJoystick != null)
            {
                petMoveJoystick.SetActive(false);
            }
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Return();
            }
        }

        #endregion



        #region Methods

        public void Init(LevelHeader headerToLoad)
        {
            LevelsContainer.LoadData();
            CurrentIndex = LevelsContainer.GetLevelIndex(headerToLoad);

            header = headerToLoad;
            CurrentMode = headerToLoad.mode;

            editorLevelConsole.Initialize();
            IngameCamera.Instance.Initialize();

            level.Initialize();
            LoadLevel();
        }
        

        private void Replay()
        {
            petMoveJoystick.SetActive(false);

            TimeUtility.Clear();
            editorLevelConsole.Clear();

            level.FinishPlayLevel();
            LoadLevel();
        }


        private void Return()
        {
            level.FinishPlayLevel();

            IngameCamera.Instance.Deinitialize();
            editorLevelConsole.Deinitialize();

            OnReturnRequest?.Invoke(header);
        }


        private void InvokePet()
        {
            PetSkinType petSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin;
            GameServices.Instance.PetsService.InvokeController.InvokePetForLevel(petSkinType);
        }


        private void PetInvokeComponent_OnShouldInvokePetForLevel(Pet pet)
        {
            PetsInputLevelController petsInputLevel = GameServices.Instance.LevelControllerService.PetsInputLevelController;
            petsInputLevel.SetupJoystick(petMoveJoystick);

            petMoveJoystick.SetActive(true);
        }


        private void LoadLevel()
        {
            level.LoadLevel(CurrentMode, CurrentIndex, true, WeaponType.None, GameMode.None);
            level.ForcePlay();

            projectilesCounter.text = header.projectilesCount.ToString();
        }

        #endregion
    }
}
