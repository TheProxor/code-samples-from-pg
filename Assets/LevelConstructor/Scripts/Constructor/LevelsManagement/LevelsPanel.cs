using Drawmasters.LevelsRepository;
using System;
using Drawmasters.LevelsRepository.Editor;
using TMPro;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class LevelsPanel : MonoBehaviour
    {
        public event Action<LevelHeader, bool> OnSelectHeader;

        [SerializeField] TMP_Dropdown modeDropdown = default;
        [SerializeField] TMP_Dropdown typeDropdown = default;
        [SerializeField] LevelHeadersScroll levelsScroll = default;

        LevelHeader[] headers;
        public GameMode CurrentMode => GameModeExtension.FromNameIndex<GameMode>(modeDropdown.value);
        public LevelType CurrentType => GameModeExtension.FromNameIndex<LevelType>(typeDropdown.value); // public>?

        void Awake()
        {
            modeDropdown.AddOptions(GameModeExtension.GetNames<GameMode>());
            typeDropdown.AddOptions(GameModeExtension.GetNames<LevelType>());
            levelsScroll.OnSelect += LevelsScroll_OnSelect;

            typeDropdown.value = LevelHeaderEditorPanel.DefaultLevelType.ToNameIndex();
        }


        public void Refresh(LevelHeader selectedHeader)
        {
            modeDropdown.onValueChanged.RemoveAllListeners();
            typeDropdown.onValueChanged.RemoveAllListeners();

            GameMode mode = (selectedHeader != null) ? selectedHeader.mode : CurrentMode;
            LevelType levelType = (selectedHeader != null) ? selectedHeader.levelType : CurrentType;

            headers = ConstructorLevelsManager.GetLevelHeaders(mode, levelType);
            modeDropdown.value = mode.ToNameIndex();
            typeDropdown.value = levelType.ToNameIndex();

            levelsScroll.Set(headers, selectedHeader);

            modeDropdown.onValueChanged.AddListener((_) => Refresh(null));
            typeDropdown.onValueChanged.AddListener((_) => Refresh(null));
        }

        void LevelsScroll_OnSelect(LevelHeader header, bool isInitialization) => OnSelectHeader?.Invoke(header, isInitialization);
    }
}
