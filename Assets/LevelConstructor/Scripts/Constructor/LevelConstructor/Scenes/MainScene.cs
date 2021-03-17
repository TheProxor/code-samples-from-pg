using Core;
using Drawmasters.LevelConstructor;
using Drawmasters.LevelsRepository;
using Spine.Unity.Examples;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Modules.General.InAppPurchase;
using Drawmasters.Vibration;


namespace Drawmasters.Constructor
{
    public class MainScene : MonoBehaviour
    {
        #region Fields

        [SerializeField] private LinkScene managementScene = default;
        [SerializeField] private LinkScene editorScene = default;
        [SerializeField] private LinkScene playerScene = default;

        [SerializeField] private GameObject privacyManager = default;
        [SerializeField] private GameObject uiKitManagerPrefab = default;

        #endregion



        #region Unity Lifecycle

        void Awake()
        {
            // init queue  
            //var initService = new InitializeService();

            //initService.Initialize(Load, privacyManager);

            //initService.Initialize(null, privacyManager); TODO: that broke axis handlers for editor level object

            InitializationUtil.Initialize(() =>
            {
                _ = GameServices.Instance;

                VibrationManager.Initialize();
                Content.Instance.Initialize();
                TouchManager.Instance.Initialize();
                EffectManager.Instance.Initialize();

                SkeletonRagdoll2D.AttachmentBoundingBoxNameMarker = string.Empty;

                LevelsManagement.OnOpeneLevelRequest += (header) => StartCoroutine(ManagementToEditor(header));

                LevelEditor.OnPlayRequest += (header) => StartCoroutine(EditorToPlayer(header));
                LevelEditor.OnReturnRequest += (header) => StartCoroutine(EditorToManagement(header));

                LevelPlayer.OnReturnRequest += (header) => StartCoroutine(PlayerToEditor(header));

                StartCoroutine(MainToManagement());
            }, privacyManager, uiKitManagerPrefab);

        }

        #endregion



        #region Methods

        private void PerformTransition(LinkScene leavedScene, LinkScene newScene)
        {
            if (leavedScene != null)
            {
                EditorSceneManager.UnloadSceneAsync(leavedScene.Path);
            }

            EditorSceneManager.LoadSceneAsyncInPlayMode(newScene.Path,
                new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });
        }


        private IEnumerator ManagementToEditor(LevelHeader header)
        {
            PerformTransition(managementScene, editorScene);

            while (true)
            {
                yield return new WaitForEndOfFrame();
                LevelEditor levelEditor = FindObjectOfType<LevelEditor>();
                if (levelEditor != null)
                {
                    levelEditor.Init(header);
                    break;
                }
            }
        }


        private IEnumerator EditorToPlayer(LevelHeader header)
        {
            PerformTransition(editorScene, playerScene);

            while (true)
            {
                yield return new WaitForEndOfFrame();
                LevelPlayer level = FindObjectOfType<LevelPlayer>();
                if (level != null)
                {
                    level.Init(header);
                    break;
                }
            }
        }


        private IEnumerator EditorToManagement(LevelHeader header)
        {
            PerformTransition(editorScene, managementScene);

            while (true)
            {
                yield return new WaitForEndOfFrame();
                LevelsManagement level = FindObjectOfType<LevelsManagement>();
                if (level != null)
                {
                    level.Init(header);
                    break;
                }
            }
        }


        private IEnumerator MainToManagement()
        {
            PerformTransition(null, managementScene);

            while (true)
            {
                yield return new WaitForEndOfFrame();
                LevelsManagement level = FindObjectOfType<LevelsManagement>();
                if (level != null)
                {
                    level.Init(null);
                    break;
                }
            }
        }


        private IEnumerator PlayerToEditor(LevelHeader header)
        {
            PerformTransition(playerScene, editorScene);

            while (true)
            {
                yield return new WaitForEndOfFrame();
                LevelEditor levelEditor = FindObjectOfType<LevelEditor>();
                if (levelEditor != null)
                {
                    levelEditor.Init(header);
                    break;
                }
            }
        }

        #endregion
    }
}
