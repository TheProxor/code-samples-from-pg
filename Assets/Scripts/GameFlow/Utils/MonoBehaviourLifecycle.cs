using System;
using System.Collections;
using UnityEngine;


public class MonoBehaviourLifecycle : MonoBehaviour
{
    #region Fields

    public static event Action<bool> OnPaused;
    public static event Action<float> OnFixedUpdate;
    public static event Action<float> OnUpdate;
    public static event Action<float> OnLateUpdate;
    public static event Action OnQuit;

    private static MonoBehaviourLifecycle instance;

    #endregion



    #region Unity lifecycle

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        GameObject go = new GameObject("MonoBehaviourLifecycle");
        go.AddComponent<MonoBehaviourLifecycle>();
        DontDestroyOnLoad(go);
    }


    public static Coroutine PlayCoroutine(IEnumerator routine) => instance.StartCoroutine(routine);


    public static bool StopPlayingCorotine(Coroutine routine)
    {
        if (routine != null)
        {
            instance.StopCoroutine(routine);

            return true;
        }

        return false;
    }


    public static void Exit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void Awake() => instance = this;


    private void FixedUpdate() => OnFixedUpdate?.Invoke(Time.fixedDeltaTime);
    

    private void Update() => OnUpdate?.Invoke(Time.deltaTime);


    private void LateUpdate() => OnLateUpdate?.Invoke(Time.deltaTime);
    
    
    private void OnApplicationPause(bool isPaused) => OnPaused?.Invoke(isPaused);
    

    private void OnApplicationQuit() => OnQuit?.Invoke();
   

    private void Reset()
    {
        if (GetType() == typeof(MonoBehaviourLifecycle))
        {
            CustomDebug.Log("MonoBehaviourLifecycle is created automatically before scene load");
            DestroyImmediate(this);
        }
    }

    #endregion
}
