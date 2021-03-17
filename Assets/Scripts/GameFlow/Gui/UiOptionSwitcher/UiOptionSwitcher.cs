using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public abstract class UiOptionSwitcher : MonoBehaviour
    {
        #region Fields

        [SerializeField] protected List<UiOptionSwitcherData> uiOptionSwitcherData = default;
        
        [SerializeField] private Button buttonSelectPrev = default;
        [SerializeField] private Button buttonSelectNext = default;

        private int selected;

        #endregion



        #region Properties

        public int Selected
        {
            get => selected;
            set
            {
                selected = value;

                ApplyContentChanges();
            }
        }

        #endregion



        #region Unity Lifecycle

        protected virtual void OnEnable()
        {
            buttonSelectPrev.onClick.AddListener(ButtonSelectPrev_OnClick);
            buttonSelectNext.onClick.AddListener(ButtonSelectNext_OnClick);
        }


        protected virtual void OnDisable()
        {
            buttonSelectPrev.onClick.RemoveListener(ButtonSelectPrev_OnClick);
            buttonSelectNext.onClick.RemoveListener(ButtonSelectNext_OnClick);
        }

        #endregion



        #region Methods

        public virtual void SwitchTo(int target)
        {
            int rangeMinIndex = 0;
            int rangeMaxIndex = uiOptionSwitcherData.Count - 1;

            if (target < rangeMinIndex)
            {
                target = rangeMaxIndex;
            }

            if (target > rangeMaxIndex)
            {
                target = rangeMinIndex;
            }

            Selected = target;
        }
        

        private void SwitchPrev() =>
            SwitchTo(Selected - 1);


        private void SwitchNext() =>
            SwitchTo(Selected + 1);


        protected abstract void ApplyContentChanges();

        #endregion



        #region Event Handlers

        private void ButtonSelectPrev_OnClick() =>
            SwitchPrev();


        private void ButtonSelectNext_OnClick() =>
            SwitchNext();
        

        #endregion
    }
}