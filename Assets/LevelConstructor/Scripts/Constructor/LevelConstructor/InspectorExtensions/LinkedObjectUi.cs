using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class LinkedObjectUi : MonoBehaviour
    {
        #region Fields

        public event Action<LinkedObjectUi> OnRemove;

        [SerializeField] private Button removeButton = null;
        [SerializeField] private TextMeshProUGUI linkedObjectLabel = null;

        #endregion



        #region Properties

        public EditorLevelObject LinkedObject { get; private set; }

        #endregion



        #region Unity lifecycle

        private void Awake() => removeButton.onClick.AddListener(RemoveButton_OnClick);

        private void OnDestroy() => removeButton.onClick.RemoveListener(RemoveButton_OnClick);

        #endregion



        #region Methods

        public void Init(EditorLevelObject linkedObject)
        {
            LinkedObject = linkedObject;

            linkedObjectLabel.text = linkedObject.name;
        }


        private void RemoveButton_OnClick() => OnRemove?.Invoke(this);

        #endregion
    }
}
