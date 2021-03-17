using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class MonolithLevelObjectExtension : InspectorExtensionBase
    {
        #region Fields

        private const string IsOpenEndedInputTitle = "Is OpenEnded";

        [SerializeField] private BoolInputUi isOpenEndedInput = default;

        private EditorLevelObjectMonolith editorLevelObjectMonolith;

        #endregion



        #region Methods

        public override void Init(EditorLevelObject levelObject)
        {
            editorLevelObjectMonolith = levelObject.GetComponent<EditorLevelObjectMonolith>();
            
            isOpenEndedInput.Init(IsOpenEndedInputTitle, editorLevelObjectMonolith.IsOpenEnded);
        }


        protected override void SubscribeOnEvents()
        {
            isOpenEndedInput.OnValueChange += IsOpenEndedInput_OnValueChange;
        }


        protected override void UnsubscribeFromEvents()
        {
            isOpenEndedInput.OnValueChange -= IsOpenEndedInput_OnValueChange;
        }

        #endregion



        #region Events handlers

        private void IsOpenEndedInput_OnValueChange(bool isActive)
        {
            if (editorLevelObjectMonolith != null)
            {
                editorLevelObjectMonolith.SetOpenEndedState(isActive);
            }
        }

        #endregion
    }
}
