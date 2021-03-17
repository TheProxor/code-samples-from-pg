using Drawmasters.LevelsRepository;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class LevelHeaderEditorPanel : MonoBehaviour
    {
        public const int DeafultProjectilesCount = 3;
        public const int DefaultStagesCount = 3;
        public const LevelType DefaultLevelType = LevelType.Simple;

        public event Action<LevelHeader> OnApply;
        public event Action OnShouldRefreshInfo;

        [SerializeField] TMP_InputField titleInput = default;
        [SerializeField] TMP_Dropdown modeDropdown = default;
        [SerializeField] TMP_Dropdown typeDropdown = default;
        [SerializeField] TMP_InputField projectilesCountLabel = default;

        [SerializeField] TMP_Dropdown weaponDropdown = default;
        [SerializeField] Toggle enableToggle = default;
        [SerializeField] Button applyButton = default;

        [Header("Stage logic")]
        [SerializeField] private SliderInputUi stageSlider = default;
        [SerializeField] private TMP_InputField stagesCountLabel = default;
        [SerializeField] private TMP_InputField stagesProjectilesCountLabel = default;

        private int currentStage;
        private LevelHeader savedHeader;

        void Awake()
        {
            applyButton.onClick.AddListener(Apply);
            modeDropdown.AddOptions(GameModeExtension.GetNames<GameMode>());
            typeDropdown.AddOptions(new List<string>(Enum.GetNames(typeof(LevelType))));
            weaponDropdown.AddOptions(GameModeExtension.GetNames<WeaponType>());
        }


        public void Set(LevelHeader value)
        {
            gameObject.SetActive(value != null);
            savedHeader = value;

            modeDropdown.onValueChanged.RemoveAllListeners();
            typeDropdown.onValueChanged.RemoveAllListeners();
            weaponDropdown.onValueChanged.RemoveAllListeners();
            stageSlider.RemoveAllListeners();

            if (value == null)
            {
                return;
            }

            titleInput.text = value.title;
            modeDropdown.value = value.mode.ToNameIndex();
            typeDropdown.value = value.levelType.ToNameIndex();
            weaponDropdown.value = value.weaponType.ToNameIndex();
            projectilesCountLabel.text = value.projectilesCount.ToString();
            stagesCountLabel.text = value.stagesCount.ToString();
            enableToggle.isOn = !value.isDisabled;

            modeDropdown.onValueChanged.AddListener((int _) => OnShouldRefreshInfo?.Invoke());
            typeDropdown.onValueChanged.AddListener((int _) => OnShouldRefreshInfo?.Invoke());
            weaponDropdown.onValueChanged.AddListener((int _) => OnShouldRefreshInfo?.Invoke());

            if (value.levelType == LevelType.Boss)
            {
                stageSlider.MarkWholeNumbersOnly();
                //currentStage = 0;
                stageSlider.Init("STAGE", currentStage, 0, value.stagesCount - 1);

                stageSlider.OnValueChange += (v) =>
                {
                    currentStage = (int)v;

                    RefreshStageProjectilesCount();
                };

                RefreshStageProjectilesCount();
            }

            projectilesCountLabel.transform.parent.gameObject.SetActive(value.levelType != LevelType.Boss);
            stagesCountLabel.transform.parent.gameObject.SetActive(value.levelType == LevelType.Boss);
            stagesProjectilesCountLabel.transform.parent.gameObject.SetActive(value.levelType == LevelType.Boss);
            stageSlider.gameObject.SetActive(value.levelType == LevelType.Boss);


            void RefreshStageProjectilesCount()
            {
                if (value.stagesCount != 0)
                {
                    Array.Resize(ref value.stageProjectilesCount, value.stagesCount);
                    string text = currentStage < value.stageProjectilesCount.Length ?
                        value.stageProjectilesCount[currentStage].ToString() : string.Empty;

                    stagesProjectilesCountLabel.text = text;
                }
            }
        }


        public static WeaponType DeafultWeaponType(GameMode mode) => Content.Management.DeafultWeaponType(mode);


        public LevelHeader GetCurrentHeader()
        {
            LevelHeader header = ScriptableObject.CreateInstance<LevelHeader>();
            header.title = titleInput.text;
            header.mode = GameModeExtension.FromNameIndex<GameMode>(modeDropdown.value);
            header.levelType = GameModeExtension.FromNameIndex<LevelType>(typeDropdown.value);
            header.weaponType = GameModeExtension.FromNameIndex<WeaponType>(weaponDropdown.value);
            header.projectilesCount = int.Parse(projectilesCountLabel.text);
            header.stagesCount = int.Parse(stagesCountLabel.text);

            if (header.levelType == LevelType.Boss)
            {
                header.stageProjectilesCount = savedHeader.stageProjectilesCount;
                Array.Resize(ref header.stageProjectilesCount, header.stagesCount);

                if (currentStage < header.stageProjectilesCount.Length)
                {
                    header.stageProjectilesCount[currentStage] = int.Parse(stagesProjectilesCountLabel.text);
                }
            }

            header.isDisabled = !enableToggle.isOn;

            return header;
        }


        void Apply() => OnApply?.Invoke(GetCurrentHeader());
    }
}
