using Drawmasters.LevelsRepository;
using Drawmasters.LevelsRepository.Editor;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class LevelSelectorCanvas : MonoBehaviour
    {
        public static event Action<LevelHeader> OnOpenLevelClick;

        const float SecondsForDoubleClick = 0.8f;

        [SerializeField] LevelsPanel levelsPanel = default;
        [SerializeField] Button newLevelButton = default;
        [SerializeField] LevelHeaderEditorPanel editorPanel = default;
        [Space]
        [SerializeField] GameObject functionsPanel = default;
        [SerializeField] Button openButton = default;
        [SerializeField] Button cloneButton = default;
        [SerializeField] Button deleteButton = default;
        [Space]
        [SerializeField] CreateLevelCanvas createLevelPopup = default;

        DateTime previousClickTime = DateTime.MinValue;

        LevelHeader current;

        void Awake()
        {
            levelsPanel.OnSelectHeader += Levels_OnSelectHeader;
            editorPanel.OnApply += Editor_OnChange;
            newLevelButton.onClick.AddListener(NewLevel_OnClick);
            openButton.onClick.AddListener(() => OnOpenLevelClick?.Invoke(current));
            cloneButton.onClick.AddListener(CloneButton_OnClick);
            deleteButton.onClick.AddListener(DeleteButton_OnClick);
        }


        public void Init(LevelHeader header) => levelsPanel.Refresh(header);


        void Levels_OnSelectHeader(LevelHeader header, bool isInitialization)
        {
            if (!isInitialization && 
                current == header && 
                header != null)
            {
                if ((DateTime.Now - previousClickTime).TotalSeconds < SecondsForDoubleClick)
                {
                    OnOpenLevelClick?.Invoke(current);
                }

                previousClickTime = DateTime.Now;
            }

            current = header;
            editorPanel.Set(current);
            functionsPanel.SetActive(current != null);

            if (header != null &&
                header.mode != GameMode.None)
            {
                EditorObjectsContainer.CurrentGameMode = header.mode;
                EditorObjectsContainer.CurrentWeaponType = header.weaponType;
                EditorObjectsContainer.UpdatePrefabs();
            }
        }


        void Editor_OnChange(LevelHeader data)
        {
            ConstructorLevelsManager.SetHeader(current, data);
            levelsPanel.Refresh(current);
        }


        void NewLevel_OnClick() => createLevelPopup.Show(levelsPanel.CurrentMode, CreateLevel_OnHide);


        void CreateLevel_OnHide(LevelHeader header)
        {
            if (header != null)
            {
                ConstructorLevelsManager.CreateLevel(header);
                levelsPanel.Refresh(header);
            }
        }


        void CloneButton_OnClick()
        {
            LevelHeader header = ScriptableObject.CreateInstance<LevelHeader>();
            ConstructorLevelsManager.SetHeader(header, current);
            header.title += " (Clone)";
            ConstructorLevelsManager.CreateLevel(header, ConstructorLevelsManager.GetLevelData(current));
            levelsPanel.Refresh(header);
        }


        void DeleteButton_OnClick()
        {
            ConstructorLevelsManager.RemoveLevel(current);
            levelsPanel.Refresh(null);
        }
    }
}
