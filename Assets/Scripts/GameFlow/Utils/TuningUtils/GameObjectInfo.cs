using UnityEngine;


[ExecuteInEditMode]
public class GameObjectInfo : MonoBehaviour
{
    #if UNITY_EDITOR

    #region Fields

    private static string messagePrefix = "GoInfo : ";

    #endregion



    #region Methods

    [Sirenix.OdinInspector.Button]
    void PrintGlobalPosition()
    {
        string position = "global position " + transform.position;

        DisplayMessage(position);
    }


    [Sirenix.OdinInspector.Button]
    void PrintGlobalRotation()
    {
        string rotation = "global rotation " + transform.rotation.eulerAngles;

        DisplayMessage(rotation);
    }


    [Sirenix.OdinInspector.Button]
    void PrintGlobalScale()
    {
        string scale = "global scale " + transform.lossyScale;

        DisplayMessage(scale);
    }


    void DisplayMessage(string message)
    {
        print(messagePrefix + message);
    }

    #endregion

    #endif
}
