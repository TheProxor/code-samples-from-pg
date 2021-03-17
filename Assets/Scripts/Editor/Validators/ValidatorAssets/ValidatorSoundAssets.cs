using System.Collections.Generic;
using Modules.Sound;
using UnityEditor;
using UnityEngine;


namespace Drawmasters.Editor
{
    public class ValidatorSoundAssets : ValidatorAssets
    {
        #region Properties

        protected override string AssetName => "SoundManager"; 

        #endregion


        #region Protected Methods

        protected override bool ValidateAsset(string prefabName)
        {
            string[] prefab = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
            if (prefab == null || prefab.Length == 0)
            {
                Debug.LogError($"{prefabName} prefab not found");
                return false;
            }

            SoundManager soundManager =
                AssetDatabase.LoadAssetAtPath<SoundManager>(AssetDatabase.GUIDToAssetPath(prefab[0]));
            SerializedObject soundManagerObject = new SerializedObject(soundManager);

            SerializedProperty containers = soundManagerObject.FindProperty("containers");

            if (containers == null || !containers.isArray)
            {
                Debug.LogError($"{prefabName} not found property \"containers\"");
                return false;
            }

            if (containers.arraySize == 0)
            {
                Debug.LogError($"{prefabName} property \"containers\" is empty");
                return false;
            }

            List<MusicCategory> musicCategorys = new List<MusicCategory>(containers.arraySize);
            bool result = true;
            for (var i = 0; i < containers.arraySize; i++)
            {
                SerializedProperty element = containers.GetArrayElementAtIndex(i);
                object elementObject = AttributeUtility.GetParentObjectFromProperty(element);
                SoundManager.CategoryData categoryData = elementObject as SoundManager.CategoryData;
                if (categoryData == null)
                {
                    Debug.LogError($"Element index={i} error convert to SoundManager.CategoryData");
                    result = false;
                    continue;
                }

                if (musicCategorys.Contains(categoryData.category))
                {
                    Debug.LogError($"Duplication category {categoryData.category}");
                }

                musicCategorys.Add(categoryData.category);

                SoundConfigsContainer soundConfigsContainer = categoryData.itemsContainer;
                if (soundConfigsContainer == null)
                {
                    Debug.LogError($"category {categoryData.category} itemsContainer is null");
                    result = false;
                    continue;
                }

                List<AssetLink> links = soundConfigsContainer.Links;
                if (links == null || links.Count == 0)
                {
                    Debug.LogError($"category {categoryData.category} soundConfigsContainer.links is null or empty");
                    result = false;
                    continue;
                }


                foreach (var link in links)
                {
                    if (link == null)
                    {
                        Debug.LogError($"{soundConfigsContainer.name} link index={i} is null");
                        result = false;
                        continue;
                    }

                    GameObject assetLinkObGameObject = link.GetAsset() as GameObject;
                    if (assetLinkObGameObject == null)
                    {
                        Debug.LogError($"{link.Name} GameObject not found");
                        result = false;
                        continue;
                    }

                    SoundConfig soundConfig = assetLinkObGameObject.GetComponent<SoundConfig>();
                    if (soundConfig == null)
                    {
                        Debug.LogError($"{link.Name} SoundConfig not found");
                        result = false;
                        continue;
                    }

                    if (soundConfig.Clip == null)
                    {
                        Debug.LogError($"{link.Name} AudioClip is null");
                        result = false;
                        continue;
                    }

                    link.Unload();
                }
            }

            return result;
        }

        #endregion
    }
}
