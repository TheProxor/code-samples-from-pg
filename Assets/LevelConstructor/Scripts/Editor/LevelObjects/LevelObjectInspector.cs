using Drawmasters.Levels;
using Drawmasters.LevelConstructor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;


namespace Drawmasters.Editor
{
    [CustomEditor(typeof(LevelObject), true)]
    public class LevelObjectInspector : OdinEditor
    {
        #region Properties

        const string EditorObjectsFolder = "Assets/LevelConstructor/Prefabs/Constructor/Objects";

        int Index
        {
            get
            {
                return serializedObject.FindProperty("index").intValue;
            }
            set
            {
                serializedObject.FindProperty("index").intValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion



        #region Public Methods

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                if (Index < 0)
                {
                    if (GUILayout.Button("Set index"))
                    {
                        SetIndex();
                    }
                }

                if (!EditorObjectsContainer.Prefabs.Exists((editorObject) => editorObject.Index == Index))
                {
                    if (GUILayout.Button("Create Editor Object"))
                    {
                        CreateEditorObject();
                    }
                }
            }
        }

        #endregion;



        #region Private Methods

        void SetIndex()
        {
            string prefabPath = PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath;
            LevelObject prefab = AssetDatabase.LoadAssetAtPath<LevelObject>(prefabPath);

            Index = Content.Storage.AddLevelObject(prefab);
            AssetDatabase.SaveAssets();
        }


        void CreateEditorObject()
        {
            string prefabPath = PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath;
            GameObject prefab = (GameObject)AssetDatabase.LoadMainAssetAtPath(prefabPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            EditorLevelObject editorLevelObject;

            switch ((LevelObject)target)
            {
                case EnemyBoss enemyBoss:
                    editorLevelObject = instance.AddComponent<EditorEnemyBossLevelObject>();
                    break;

                case Spikes spikes:
                    editorLevelObject = instance.AddComponent<EditorSpikes>();
                    break;

                case PhysicalLevelObject physicalLevelObject:
                    editorLevelObject = instance.AddComponent<EditorPhysicalLevelObject>();
                    break;

                case LevelTarget levelTarget:
                    editorLevelObject = instance.AddComponent<EditorLevelTargetObject>();
                    break;

                case LiquidLevelObject liquid:
                    editorLevelObject = instance.AddComponent<EditorLiquidLevelObject>();
                    break;

                case VisualLevelObject visualLevelObject:
                    editorLevelObject = instance.AddComponent<EditorVisualLevelObject>();
                    break;

                case Rope ropeLevelObject:
                    editorLevelObject = instance.AddComponent<EditorRope>();
                    break;

                default:
                    editorLevelObject = instance.AddComponent<EditorLevelObject>();
                    break;
            }

            DestroyImmediate(editorLevelObject.GetComponent<LevelObject>(), true);
            editorLevelObject.Index = prefab.GetComponent<LevelObject>().Index;
            editorLevelObject.IsStaticByDefault = prefab.GetComponent<LevelObject>().IsStaticByDefault;

            int editorObjectLayer = LayerMask.NameToLayer(PhysicsLayers.EditorLevelObject);
            instance.SetLayerRecursively(editorObjectLayer);

            PrefabUtility.SaveAsPrefabAsset(instance, $"{EditorObjectsFolder}/Constructor_{prefab.name}.prefab");
            AssetDatabase.Refresh();

            EditorObjectsContainer.UpdatePrefabs();

            DestroyImmediate(instance);
        }

        #endregion
    }
}
