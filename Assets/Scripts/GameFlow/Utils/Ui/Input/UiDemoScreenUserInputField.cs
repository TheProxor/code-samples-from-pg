using UnityEngine;
using TMPro;


namespace Drawmasters.Utils.Ui
{
    [RequireComponent(typeof(Canvas))]
    public class UiDemoScreenUserInputField : MonoBehaviour
    {
        [SerializeField] private UserInputField userInputField = default;
        [SerializeField] private TMP_Text result = default;
        [SerializeField] private TMP_Text eventText = default;


        private void Awake()
        {
            userInputField.Initialize();
            userInputField.OnInputSubmited += UserInputField_OnInputSubmited;
        }


        private void OnDestroy()
        {

            userInputField.Deinitialize();
            userInputField.OnInputSubmited -= UserInputField_OnInputSubmited;
        }


        private void UserInputField_OnInputSubmited(string obj)
        {
            result.text = obj;
            eventText.text = "OnInputSubmited";
        }
    }
}
