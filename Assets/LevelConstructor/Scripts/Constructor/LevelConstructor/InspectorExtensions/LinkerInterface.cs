using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class LinkerInterface : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Button setLinkButton = null;
        [SerializeField] private Transform linkedObjectsUiRoot = null;
        [SerializeField] private LinkedObjectUi linkedObjectUiPrefab = null;
        [SerializeField] private LayoutElement layoutElement = null;
        [SerializeField] private float defaultHeight = 0.0f;
        [SerializeField] private float additionalHeightPerLinkedObject = 0.0f;

        List<LinkedObjectUi> linkedObjects = new List<LinkedObjectUi>();

        EditorLevelObject selectedObject;

        #endregion



        #region Unity lifecycle

        private void Awake() => setLinkButton.onClick.AddListener(() => LinkSettingRequest.Register(true));


        private void OnEnable() => LinkSettingFinished.Subscribe(OnLinkSettingFinished);


        private void OnDisable()
        {
            LinkSettingRequest.Register(false);
            LinkSettingFinished.Unsubscribe(OnLinkSettingFinished);
        }

        #endregion



        #region Methods

        public void Init(EditorLevelObject levelObject)
        {
            LinkSettingRequest.Register(false);

            UpdateInterface(levelObject);
        }


        private void UpdateInterface(EditorLevelObject levelObject)
        {
            foreach (var linkedObject in linkedObjects)
            {
                Destroy(linkedObject.gameObject);
            }

            linkedObjects.Clear();

            selectedObject = levelObject;

            foreach (var linkedObject in EditorLinker.GetLinks(levelObject))
            {
                AddLinkUi(linkedObject);
            }

            RecalculateHeight();
        }


        private void AddLinkUi(EditorLevelObject linkedObject)
        {
            LinkedObjectUi linkedObjectUi = Instantiate(linkedObjectUiPrefab, linkedObjectsUiRoot);
            linkedObjectUi.Init(linkedObject);
            linkedObjectUi.OnRemove += RemoveLink;
            linkedObjectUi.gameObject.SetActive(true);
            linkedObjects.Add(linkedObjectUi);

            RecalculateHeight();

            LinkSettingRefreshed.Register();
        }


        private void RemoveLink(LinkedObjectUi linkedObjectUi)
        {
            linkedObjects.Remove(linkedObjectUi);

            selectedObject.RemoveLink(linkedObjectUi.LinkedObject);

            Destroy(linkedObjectUi.gameObject);

            RecalculateHeight();

            LinkSettingRefreshed.Register();
        }


        private void RecalculateHeight() => layoutElement.preferredHeight = defaultHeight + linkedObjects.Count * additionalHeightPerLinkedObject;


        private void OnLinkSettingFinished() => UpdateInterface(selectedObject);

        #endregion
    }
}
