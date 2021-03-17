using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class UiLanguageSwitcher : UiOptionSwitcher
    {
        #region Methods

        protected override void OnEnable()
        {
            base.OnEnable();

            ValidateAndFixData();
            SetCurrentSelected();
        }


        protected override void ApplyContentChanges()
        {
            UiOptionSwitcherData currentData = uiOptionSwitcherData[Selected];

            string currentLanguage = uiOptionSwitcherData[Selected].optionName;

            if (!LocalizationManager.HasLanguage(currentLanguage))
            {
                CustomDebug.Log($"Language <b>{currentLanguage}</b> is not exists in I2 Languages!");
                return;
            }

            LocalizationManager.CurrentLanguage = currentLanguage;
        }


        private void SetCurrentSelected() =>
            Selected = LocalizationManager.GetAllLanguages().FindIndex(x => x == LocalizationManager.CurrentLanguage);


        [ContextMenu("Validate and fix Data")]
        private void ValidateAndFixData()
        {
            string[] languages = LocalizationManager.GetAllLanguages().ToArray();

            RemoveMissing();
            AddMissing();
           
            void AddMissing()
            {
                foreach (var language in languages)
                {
                    if (uiOptionSwitcherData.FindIndex(x => x.optionName == language) < 0)
                    {
                        UiOptionSwitcherData languageItem = new UiOptionSwitcherData()
                        {
                            optionName = language
                        };

                        uiOptionSwitcherData.Add(languageItem);
                    }
                }
            }


            void RemoveMissing()
            {
                List<UiOptionSwitcherData> listToRemove = new List<UiOptionSwitcherData>(uiOptionSwitcherData.Count);
                foreach (var item in uiOptionSwitcherData)
                {
                    if (!LocalizationManager.HasLanguage(item.optionName))
                    {
                        listToRemove.Add(item);
                    }
                }

                foreach (var item in listToRemove)
                {
                    uiOptionSwitcherData.Remove(item);
                }
            }
        }

        #endregion
    }
}