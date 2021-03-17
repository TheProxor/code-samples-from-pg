using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class BonusLevelInspector : MonoBehaviour
    {
        #region Fields

        [SerializeField] private SliderInputUi stageSlider = default;

        private EditorLevel editorLevel;

        #endregion



        #region Proeprties

        public static int CurrentStage { get; private set; }

        #endregion



        #region Methods

        public void Initialize(EditorLevel _editorLevel, int stagesCount)
        {
            editorLevel = _editorLevel;

            stageSlider.MarkWholeNumbersOnly();
            stageSlider.Init("BONUS STAGE", 0, 0, stagesCount - 1);
            stageSlider.OnValueChange += StageSlider_OnValueChange;

            CurrentStage = default;
            editorLevel.ChangeBonusStage(CurrentStage);
        }


        public void Deinitialize()
        {
            stageSlider.OnValueChange -= StageSlider_OnValueChange;
        }

        #endregion



        #region Events handlers

        private void StageSlider_OnValueChange(float floatValue)
        {
            CurrentStage = (int)floatValue;

            editorLevel.ChangeBonusStage(CurrentStage);
        }

        #endregion
    }
}

