using Drawmasters.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class EditorLevelConsole : MonoBehaviour, IInitializable, IDeinitializable
    {
        #region Field

        private static readonly bool IsEnabled = false;

        private const string StartColorPattern = "<color={0}>";
        private const string EndColorTag = "</color>";

        [SerializeField] private TextMeshProUGUI consoleOutput = default;
        [SerializeField] private Button clearOutputButton = default;

        [SerializeField] private EditorLevelConsoleSettings settings = default;

        #endregion



        #region Methods

        public void Initialize()
        {
            WeaponTriggerImpuls.OnShouldLog += WeaponTriggerImpuls_OnShouldLog;
            ImpulseDestroyComponent.OnShouldLog += ImpulseDestroyComponent_OnShouldLog;
            LimbsDamageHandler.OnShouldLog += LimbsDamageLevelTargetComponent_OnShouldLog;

            ImpulsRagdollApplyLevelTargetComponent.OnShouldLog += ImpulsRagdollApplyLevelTargetComponent_OnShouldLog;
            LimbPartsImpulsLevelTargetComponent.OnShouldLog += LimbPartsImpulsLevelTargetComponent_OnShouldLog;

            consoleOutput.fontSize = settings.fontSize;

            clearOutputButton.onClick.AddListener(ClearOutputButton_OnClick);
        }


        public void Deinitialize()
        {
            WeaponTriggerImpuls.OnShouldLog -= WeaponTriggerImpuls_OnShouldLog;
            ImpulseDestroyComponent.OnShouldLog -= ImpulseDestroyComponent_OnShouldLog;
            LimbsDamageHandler.OnShouldLog -= LimbsDamageLevelTargetComponent_OnShouldLog;

            ImpulsRagdollApplyLevelTargetComponent.OnShouldLog -= ImpulsRagdollApplyLevelTargetComponent_OnShouldLog;
            LimbPartsImpulsLevelTargetComponent.OnShouldLog -= LimbPartsImpulsLevelTargetComponent_OnShouldLog;

            clearOutputButton.onClick.RemoveListener(ClearOutputButton_OnClick);
        }


        public void Clear()
        {
            consoleOutput.SetText(string.Empty);
        }


        private void LogText(string text, string color)
        {
            if (!IsEnabled)
            {
                return;
            }

            string startColorTag = string.Format(StartColorPattern, color);

            string colorText = startColorTag + text + EndColorTag;
            consoleOutput.SetText(consoleOutput.text + "\n" + colorText);
        }

        #endregion



        #region Events handlers

        private void WeaponTriggerImpuls_OnShouldLog(string text)
        {
            string color = settings.FindColorTag(ObjectsCollisionType.Damage);
            LogText(text, color);
        }



        private void LimbPartsImpulsLevelTargetComponent_OnShouldLog(string text)
        {
            string color = settings.FindColorTag(ObjectsCollisionType.Impuls);
            LogText(text, color);
        }


        private void ImpulsRagdollApplyLevelTargetComponent_OnShouldLog(string text)
        {
            string color = settings.FindColorTag(ObjectsCollisionType.Impuls);
            LogText(text, color);
        }


        private void LimbsDamageLevelTargetComponent_OnShouldLog(string text)
        {
            string color = settings.FindColorTag(ObjectsCollisionType.Damage);
            LogText(text, color);
        }


        private void ImpulseDestroyComponent_OnShouldLog(string text)
        {
            string color = settings.FindColorTag(ObjectsCollisionType.DestroyDamage);
            LogText(text, color);
        }


        private void ClearOutputButton_OnClick()
        {
            Clear();
        }

        #endregion
    }
}
