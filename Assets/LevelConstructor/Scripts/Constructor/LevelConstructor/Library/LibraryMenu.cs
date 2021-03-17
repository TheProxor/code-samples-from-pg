using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Drawmasters.LevelsRepository;


namespace Drawmasters.LevelConstructor
{
    public class LibraryMenu : MenuBase
    {
        #region Fields

        public static event Action<int> OnObjectSelected = delegate { };

        [SerializeField] private Transform previewRoot = null;
        [SerializeField] private LibraryObjectPreview objectPreviewPrefab = null;
        [SerializeField] private TMP_InputField searchInput = default;

        private List<LibraryObjectPreview> previews = new List<LibraryObjectPreview>();
        private LibraryObjectPreview selectedPreview;

        private LevelHeader header;

        #endregion



        #region Unity Lifecycle

        private void Awake()
        {
            searchInput.onValueChanged.AddListener((searchText) =>
            {
                previews.ForEach((preview) => preview.gameObject.SetActive(preview.Name.ToLower().Contains(searchText.ToLower())));
            });
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            LibraryObjectPreview.OnClick += LibraryObjectPreview_OnClick;
        }


        protected override void OnDisable()
        {
            base.OnDisable();

            LibraryObjectPreview.OnClick -= LibraryObjectPreview_OnClick;
        }


        private void Start()
        {
            foreach (var prefab in EditorObjectsContainer.Prefabs)
            {
                if (IsPrefabAvailable(prefab))
                {
                    LibraryObjectPreview preview = Instantiate(objectPreviewPrefab, previewRoot);
                    preview.SetPreviewTarget(prefab);
                    previews.Add(preview);
                }
            }
        }

        #endregion



        #region Methods

        public void SetupHeader(LevelHeader _header) =>
           header = _header;


        private bool IsPrefabAvailable(EditorLevelObject prefab)
        {
            bool result = true;

            switch (header.levelType)
            {
                case LevelType.Simple:
                case LevelType.Bonus:
                    result = true;
                    break;

                case LevelType.Boss:
                    result = prefab.gameObject.GetComponent<EditorEnemyBossLevelObject>() != null ||
                             prefab.gameObject.GetComponent<EditorEnemyHitmastersBossLevelObject>() != null;
                    break;

                default:
                    CustomDebug.Log($"There is no processing for <b>LevelType = {header.levelType}</b> for <b>{prefab.name}</b> prefab");
                    break;
            }

            return result; 
        }

        #endregion



        #region Events Handlers

        private void LibraryObjectPreview_OnClick(LibraryObjectPreview preview, int selectedObjectIndex)
        {
            selectedPreview?.Deselect();

            selectedPreview = preview;
            OnObjectSelected(selectedObjectIndex);
            CloseButton_OnClick();
        }

        #endregion
    }
}
