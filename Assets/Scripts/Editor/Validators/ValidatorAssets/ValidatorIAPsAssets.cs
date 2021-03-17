using Modules.InAppPurchase;
using Modules.General.InAppPurchase;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;


namespace Drawmasters.Editor
{
    public class ValidatorIAPsAssets : ValidatorAssets
    {
        #region Properties

        protected override string AssetName => "LLStoreSettings";

        #endregion


        #region Protected Methods

        protected override bool ValidateAsset(string scriptableObjectName)
        {
            bool result = true;

            if (BuildInfo.IsChinaBuild)
            {
                return result;
            }

            string[] prefab = AssetDatabase.FindAssets($"{scriptableObjectName} t:ScriptableObject");
            if (prefab == null || prefab.Length == 0)
            {
                Debug.LogError($"{scriptableObjectName} prefab not found");
                return false;
            }

            LLStoreSettings loadedScriptableObject = AssetDatabase.LoadAssetAtPath<LLStoreSettings>(AssetDatabase.GUIDToAssetPath(prefab[0]));

            string[] customDefinedKeys = typeof(IAPs).GetAllFieldsKeys<string>();
            string[] scriptableObjectDefinedKeys = LLStoreSettings.StoreItems.Select(e => e.Identifier).ToArray();

            string[] missedInScriptableObjectKeys = scriptableObjectDefinedKeys.Where(e => !Array.Exists(customDefinedKeys, c => c.Equals(e, StringComparison.InvariantCulture))).ToArray();
            foreach (var key in missedInScriptableObjectKeys)
            {
                Debug.Log($"No key = <b>{key}</b> from <b>{nameof(IAPs)}</b> had found in <b>{nameof(LLStoreSettings)}</b>.");
            }

            result &= missedInScriptableObjectKeys.Length == 0;

            string[] missedInCustomDefinedKeys = customDefinedKeys.Where(e => !Array.Exists(scriptableObjectDefinedKeys, c => c.Equals(e, StringComparison.InvariantCulture))).ToArray();
            foreach (var key in missedInCustomDefinedKeys)
            {
                Debug.Log($"No key = <b>{key}</b> from <b>{nameof(LLStoreSettings)}</b> had found in <b>{nameof(IAPs)}</b>.");
            }

            result &= missedInCustomDefinedKeys.Length == 0;

            return result;
        }

        #endregion
    }
} 
