using Drawmasters.LevelsRepository;
using Drawmasters.LevelsRepository.Editor;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class CreateLevelCanvas : MonoBehaviour
    {
        [SerializeField] private LevelHeaderEditorPanel headerEditorPanel = default;
        [SerializeField] private Button cancelButton = default;

        private Action<LevelHeader> callback;


        private void Awake()
        {
            cancelButton.onClick.AddListener(CancelButton_Click);
            headerEditorPanel.OnApply += HeaderEditorPanel_OnApply;
            headerEditorPanel.OnShouldRefreshInfo += HeaderEditorPanel_OnModeChange;
        }


        public void Show(GameMode mode, Action<LevelHeader> onHide)
        {
            gameObject.SetActive(true);

            callback = onHide;
            (int order, string title) info = GetLastLevelOrderAndTitle(mode);
            LevelHeader header = ScriptableObject.CreateInstance<LevelHeader>();
            header.mode = mode;
            header.title = info.title;
            header.projectilesCount = LevelHeaderEditorPanel.DeafultProjectilesCount;
            header.weaponType = LevelHeaderEditorPanel.DeafultWeaponType(mode);
            header.levelType = LevelHeaderEditorPanel.DefaultLevelType;
            header.stagesCount = LevelHeaderEditorPanel.DefaultStagesCount;

            headerEditorPanel.Set(header);
        }


        private void HeaderEditorPanel_OnApply(LevelHeader header)
        {
            gameObject.SetActive(false);
            callback?.Invoke(header);
        }

        private void CancelButton_Click()
        {
            gameObject.SetActive(false);
            callback?.Invoke(null);
        }


        (int, string) GetLastLevelOrderAndTitle(GameMode mode)
        {
            LevelHeader[] modeHeaders = ConstructorLevelsManager.GetLevelHeaders(mode);
            int order = modeHeaders.Length + 1;
            string title = order.ToString();

            while (Array.Exists(modeHeaders, (h) => h.title == title))
            {
                title = (int.Parse(title) + 1).ToString();
            }

            return (order, title);
        }


        private void HeaderEditorPanel_OnModeChange()
        {
            LevelHeader header = headerEditorPanel.GetCurrentHeader();
            (int order, string title) info = GetLastLevelOrderAndTitle(header.mode);
            header.title = info.title;

            headerEditorPanel.Set(header);
        }
    }
}
