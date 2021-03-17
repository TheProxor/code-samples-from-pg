using UnityEngine;
using UnityEngine.UI;


namespace JoystickPlugin
{
    public class JoystickSetterExample : MonoBehaviour
    {
        public VariableJoystick variableJoystick;
        public Text valueText;
        public Image background;
        public Sprite[] axisSprites;
        public Text angleText;

        public void ModeChanged(int index)
        {
            switch (index)
            {
                case 0:
                    variableJoystick.SetMode(JoystickType.Fixed);
                    break;
                case 1:
                    variableJoystick.SetMode(JoystickType.Floating);
                    break;
                case 2:
                    variableJoystick.SetMode(JoystickType.Dynamic);
                    break;
                default:
                    break;
            }
        }

        public void AxisChanged(int index)
        {
            switch (index)
            {
                case 0:
                    variableJoystick.AxisOptions = AxisOptions.Both;
                    background.sprite = axisSprites[index];
                    break;
                default:
                    break;
            }
        }


        private void Update()
        {
            valueText.text = "Current Value: " + variableJoystick.Direction;

            float angle = Clamp0360(Mathf.Atan2(variableJoystick.Direction.y, variableJoystick.Direction.x) * Mathf.Rad2Deg);
            angleText.text = $"Angle: {angle}";
        }


        private static float Clamp0360(float eulerAngles)
        {
            float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
            if (result < 0)
            {
                result += 360f;
            }
            return result;
        }
    }
}