using System;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class MenuBase : MonoBehaviour
    {
        #region Fields

        public event Action OnClosed = delegate { };

        [SerializeField] private GameObject root = default;
        [SerializeField] private Button closeButton = default;

        #endregion



        #region Unity Lifecycle

        protected virtual void OnEnable() => closeButton.onClick.AddListener(CloseButton_OnClick);


        protected virtual void OnDisable() => closeButton.onClick.RemoveListener(CloseButton_OnClick);

        #endregion



        #region Methods

        public void Open() => root.SetActive(true);

        #endregion



        #region Events Handlers

        protected void CloseButton_OnClick()
        {
            root.SetActive(false);

            OnClosed();
        }

        #endregion
    }
}
