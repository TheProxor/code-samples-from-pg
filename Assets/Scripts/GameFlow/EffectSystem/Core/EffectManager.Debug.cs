using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Effects
{
    public partial class EffectManager
    {
#if UNITY_EDITOR

        #region Fields

        private const string EFFECTS_DIRECTORY = "Assets/Resources/Effects";

        #endregion



        #region Editor methods

        [Sirenix.OdinInspector.Button]
        private void CheckSimilarAssets()
        {
            HashSet<string> links = new HashSet<string>();
            int removedAssetsCount = 0;

            for (int i = systemHandlersAssetsLink.Count - 1; i >= 0; i--)
            {
                AssetLink effectLink = systemHandlersAssetsLink[i];

                if (links.Contains(effectLink.assetGUID))
                {
                    CustomDebug.Log($"Removed duplicate {effectLink.Name}");

                    systemHandlersAssetsLink.RemoveAt(i);
                    removedAssetsCount++;
                }
                else
                {
                    links.Add(effectLink.assetGUID);
                }
            }

            if (removedAssetsCount == 0)
            {
                CustomDebug.Log("All assets are unique");
            }
        }


        [Sirenix.OdinInspector.Button]
        private void CheckEffectForCorrectData()
        {
            bool isAnyErrorExist = default;

            for (int i = 0; i < systemHandlersAssetsLink.Count; i++)
            {
                var loadedAsset = systemHandlersAssetsLink[i].GetAsset() as GameObject;

                if (loadedAsset.IsNull())
                {
                    CustomDebug.Log($"Asset with index: {i} could not be loaded");

                    isAnyErrorExist = true;
                }
                else if (loadedAsset.GetComponent<EffectHandler>().IsNull())
                {
                    CustomDebug.Log($"Asset {loadedAsset.name} with index: {i} don't have EffectHandler component");

                    isAnyErrorExist = true;
                }
                else
                {
                    var effectHandler = loadedAsset.GetComponent<EffectHandler>();

                    foreach (var effect in effectHandler.ParticleSystems)
                    {
                        if (effect.IsNull())
                        {
                            CustomDebug.Log($"EffectHandler {effectHandler.name} with index: {i} contains null reference particle systems!");

                            isAnyErrorExist = true;
                        }
                    }

                    foreach (var effect in effectHandler.TrailsRenderers)
                    {
                        if (effect.IsNull())
                        {
                            CustomDebug.Log($"EffectHandler {effectHandler.name} with index: {i} contains null reference trails renderers!");

                            isAnyErrorExist = true;
                        }
                    }
                }
            }

            if (!isAnyErrorExist)
            {
                CustomDebug.Log($"Vfx data is correct.");
            }
        }


        private void OnValidate()
        {
            HashSet<string> links = new HashSet<string>();

            for (int i = systemHandlersAssetsLink.Count - 1; i >= 0; i--)
            {
                AssetLink effectLink = systemHandlersAssetsLink[i];

                if (string.IsNullOrEmpty(effectLink.assetGUID))
                {
                    CustomDebug.Log($"Asset with index: {i} is NULL");
                }
                else
                {
                    if (links.Contains(effectLink.assetGUID))
                    {
                        CustomDebug.Log($"Duplicate asset found: {effectLink.Name}");
                    }
                    else
                    {
                        links.Add(effectLink.assetGUID);
                    }
                }
            }
        }

        #endregion

        #endif
    }
}
