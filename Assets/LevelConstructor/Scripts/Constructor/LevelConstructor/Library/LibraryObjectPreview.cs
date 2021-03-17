using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class LibraryObjectPreview : MonoBehaviour
    {
        #region Fields

        public static event Action<LibraryObjectPreview, int> OnClick = delegate { };

        const int SpriteScale = 128;

        [SerializeField] private Image previewImage = default;
        [SerializeField] private TextMeshProUGUI nameLabel = default;
        [SerializeField] private Button selectButton = default;
        [SerializeField] private Image selectedBackground = default;

        private int targetObjectIndex;

        #endregion



        #region Properties

        public string Name => nameLabel.text;

        #endregion



        #region Unity Lifecycle

        private void Awake()
        {
            selectButton.onClick.AddListener(SelectButton_OnClick);

            Deselect();
        }

        #endregion



        #region Methods

        public void SetPreviewTarget(EditorLevelObject target)
        {
            targetObjectIndex = target.Index;
            nameLabel.text = target.name.Replace("Constructor_", string.Empty);

            StartCoroutine(SetPreviewImage(target));
        }


        public void Deselect() => selectedBackground.gameObject.SetActive(false);


        private IEnumerator SetPreviewImage(EditorLevelObject target)
        {
            yield return new WaitWhile(() => AssetPreview.GetAssetPreview(target.gameObject) == null);

            previewImage.sprite = Sprite.Create(AssetPreview.GetAssetPreview(target.gameObject), new Rect(0, 0, SpriteScale, SpriteScale), Vector2.zero);
        }

        #endregion



        #region Events Handlers

        private void SelectButton_OnClick()
        {
            selectedBackground.gameObject.SetActive(true);
            OnClick(this, targetObjectIndex);
        }

        #endregion
    }
}
