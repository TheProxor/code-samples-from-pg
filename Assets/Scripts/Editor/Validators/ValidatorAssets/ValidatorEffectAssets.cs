using System.Collections.Generic;
using System.Linq;
using Drawmasters.Effects;
using UnityEditor;
using UnityEngine;

namespace Drawmasters.Editor
{
    public class ValidatorEffectAssets : ValidatorAssets
    {
        #region Properties

        protected override string AssetName => "EffectManager";

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

            EffectManager effectManager =
                AssetDatabase.LoadAssetAtPath<EffectManager>(AssetDatabase.GUIDToAssetPath(prefab[0]));
            SerializedObject effectManagerObject = new SerializedObject(effectManager);

            SerializedProperty systemHandlersAssetsLink = effectManagerObject.FindProperty("systemHandlersAssetsLink");

            if (systemHandlersAssetsLink == null || !systemHandlersAssetsLink.isArray)
            {
                Debug.LogError($"{prefabName} not found property \"systemHandlersAssetsLink\"");
                return false;
            }

            if (systemHandlersAssetsLink.arraySize == 0)
            {
                Debug.LogError($"{prefabName} property \"systemHandlersAssetsLink\" is empty");
                return false;
            }

            bool result = true;
            for (var i = 0; i < systemHandlersAssetsLink.arraySize; i++)
            {
                SerializedProperty element = systemHandlersAssetsLink.GetArrayElementAtIndex(i);
                object elementObject = AttributeUtility.GetParentObjectFromProperty(element);
                AssetLink assetLink = elementObject as AssetLink;
                if (assetLink == null)
                {
                    Debug.LogError($"Element index={i} error convert to AssetLink ");
                    result = false;
                    continue;
                }

                GameObject assetLinkObGameObject = assetLink.GetAsset() as GameObject;
                EffectHandler effectHandler = assetLinkObGameObject.GetComponent<EffectHandler>();
                if (effectHandler == null)
                {
                    Debug.LogError($"{assetLink.Name} EffectHandler prefab not found");
                    result = false;
                    continue;
                }

                if (effectHandler.ParticleSystems == null || effectHandler.ParticleSystems.Count == 0)
                {
                    Debug.LogError($"{assetLink.Name} ParticleSystems is null or empty");
                    result = false;
                    continue;
                }

                IEnumerable<ParticleSystem> items = effectHandler.ParticleSystems.Where(x => x == null);
                if (items.Any())
                {
                    Debug.LogError($"{assetLink.Name} ParticleSystems found null element");
                    result = false;
                }
                assetLink.Unload();
            }
            return result;
        }
        
        #endregion
    }
}
