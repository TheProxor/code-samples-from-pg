using DG.Tweening;
using Drawmasters.ServiceUtil;
using Modules.UiKit;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Modules.General;
using Modules.General.Abstraction;
using UnityEngine;


namespace Drawmasters
{
    public class ApplicationManager : SingletonMonoBehaviour<ApplicationManager>
    {
        #region Fields

        public static event Action OnApplicationStarted;
        
        [SerializeField] private List<GameObject> managersPrefabs = default;
        
        [SerializeField][Required] private GameObject privacyManager = default;
        [SerializeField][Required] private GameObject uiKitManagerPrefab = default;

        [SerializeField] [Required] private LoaderScreen loaderScreen = default;

        private readonly List<IFixedUpdatable> fixedUpdatableManagers = new List<IFixedUpdatable>();
        private readonly List<IUpdatable> updatableManagers = new List<IUpdatable>();
        private readonly List<ILateUpdatable> lateUpdatableManagers = new List<ILateUpdatable>();

        private bool isApplicationStarted;


        #endregion


         
        #region Unity lifecycle

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);

            Input.multiTouchEnabled = false;
            
            DOTween.Init();

            #if UNITY_ANDROID
                LLActivity.SetDebugEnable(CustomDebug.Enable);
            #endif

            Application.targetFrameRate = 60;
            
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            ShowLoader();
            
            // BackgroundMonitor.WakeUp();

            Action onInitilized = () =>
            {
                //HACK
                Services.CreateServiceSingleton<IUiAdsController, FakeUiAdsController>();
                Services.GetService<IUiManager>().UiAdsController = Services.GetService<IUiAdsController>();
                
                //HACK
                _ = GameServices.Instance;

                CreateManagers(managersPrefabs);

                // var i  = Services.GetService<INotificationManager>();
                // CustomDebug.Log(i);

                InitializationUtil.InitializeIngame();
                MonoBehaviourLifecycle.OnQuit += MonoBehaviourLifecycle_OnQuit;

                HideLoader(OnStarted);
            };

            InitializationUtil.Initialize(onInitilized, privacyManager, uiKitManagerPrefab);
        }


        private void OnEnable()
        {
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
            MonoBehaviourLifecycle.OnFixedUpdate += MonoBehaviourLifecycle_OnFixedUpdate;
            MonoBehaviourLifecycle.OnLateUpdate += MonoBehaviourLifecycle_OnLateUpdate;
        }


        private void OnDisable()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            MonoBehaviourLifecycle.OnFixedUpdate -= MonoBehaviourLifecycle_OnFixedUpdate;
            MonoBehaviourLifecycle.OnLateUpdate -= MonoBehaviourLifecycle_OnLateUpdate;
        }

        #endregion



        #region Loader methods

        private void ShowLoader() =>
            loaderScreen.Show();


        private void HideLoader(Action onLoaderHide)
        {
            loaderScreen.OnLoaderHide += onLoaderHide;
            loaderScreen.Hide();
        }

        #endregion



        #region Private methods

        private void OnStarted()
        {
            isApplicationStarted = true;
            
            OnApplicationStarted?.Invoke();
        }
        

        private void CreateManagers(List<GameObject> managers)
        {
            List<IInitializable> initializableManagers = new List<IInitializable>();

            for (int i = 0, n = managers.Count; i < n; i++)
            {
                GameObject currentManagerPrefab = managers[i];

                if (currentManagerPrefab != null)
                {
                    GameObject currentManager = Instantiate(currentManagerPrefab, Vector3.zero, Quaternion.identity, transform);
                    currentManager.name = currentManagerPrefab.name;

                    MonoBehaviour manager = currentManager.GetComponent<MonoBehaviour>();

                    if (manager is IInitializable initializable)
                    {
                        initializableManagers.Add(initializable);
                    }

                    if (manager is IFixedUpdatable fixedUpdatable)
                    {
                        fixedUpdatableManagers.Add(fixedUpdatable);
                    }

                    if (manager is IUpdatable updatable)
                    {
                        updatableManagers.Add(updatable);
                    }

                    if (manager is ILateUpdatable lateUpdatable)
                    {
                        lateUpdatableManagers.Add(lateUpdatable);
                    }
                }
                else
                {
                    CustomDebug.Log("Manager prefab in ApplicationManager by index " + i + " is NULL.");
                }
            }

            foreach (var manager in initializableManagers)
            {
                manager.Initialize();
            }
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnLateUpdate(float deltaTime)
        {
            if (isApplicationStarted)
            {
                foreach (var manager in lateUpdatableManagers)
                {
                    manager.CustomLateUpdate(deltaTime);
                }
            }
        }


        private void MonoBehaviourLifecycle_OnFixedUpdate(float deltaTime)
        {
            if (isApplicationStarted)
            {
                foreach (var manager in fixedUpdatableManagers)
                {
                    manager.CustomFixedUpdate(deltaTime);
                }
            }
        }

        
        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (isApplicationStarted)
            {
                foreach (var manager in updatableManagers)
                {
                    manager.CustomUpdate(deltaTime);
                }
            }
        }


        private void MonoBehaviourLifecycle_OnQuit()
        {
            MonoBehaviourLifecycle.OnQuit -= MonoBehaviourLifecycle_OnQuit;

            InitializationUtil.DeinitializeIngame();
        }

        #endregion
    }
}