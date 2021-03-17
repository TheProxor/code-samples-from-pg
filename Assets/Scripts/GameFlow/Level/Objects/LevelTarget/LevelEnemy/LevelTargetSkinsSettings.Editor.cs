using System;
using Drawmasters.Utils;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Levels
{
    public partial class LevelTargetSkinsSettings : IMenuItemRefreshable
    {
        #region Editor fields

        [Header("Editor fields")]

        [SerializeField] private Shader outlineShader = default;
        [SerializeField] [Range(0.0f, 20.0f)] private float outlineWidth = default;
        [SerializeField] [Range(0.0f, 1.0f)] private float outlineThreshold = default;
        [SerializeField] [Range(0.0f, 1.0f)] private float outlineSmoothness = default;

        #endregion



        #region Menu items methods

        [Sirenix.OdinInspector.Button]
        public void RefreshFromMenuItem()
        {
            #if UNITY_EDITOR

            const string allSkeletonDataAssetsPath = "Assets/Animation/SpineAnimation/Enemy";

            SkeletonDataAsset[] loadedAssets = ResourcesUtility.LoadAssetsAtPath<SkeletonDataAsset>(allSkeletonDataAssetsPath).ToArray();

            foreach (var asset in loadedAssets)
            {
                string materialPath = UnityEditor.AssetDatabase.GetAssetPath(asset);
                materialPath = materialPath.Replace("SkeletonData.asset", "Material.mat");

                Material originalMaterial = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));

                string outlineMaterialPath = materialPath.Replace("Material.mat", "Material_Outline.mat");
                Material loadedOutlineMaterial = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath(outlineMaterialPath, typeof(Material));
                if (loadedOutlineMaterial != null)
                {
                    UnityEditor.AssetDatabase.DeleteAsset(outlineMaterialPath);
                }

                Material outlineMaterial = new Material(originalMaterial);
                outlineMaterial.shader = outlineShader;
                outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
                outlineMaterial.SetFloat("_ThresholdEnd", outlineThreshold);
                outlineMaterial.SetFloat("_OutlineSmoothness", outlineSmoothness);

                UnityEditor.AssetDatabase.CreateAsset(outlineMaterial, outlineMaterialPath);

                materialsColorsData = materialsColorsData.Put(new LevelTargetMaterialsColorsData
                {
                    key = asset,
                    originalMaterial = originalMaterial,
                    outlineMaterial = outlineMaterial
                }, e => e.originalMaterial == originalMaterial);
            }

            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.AssetDatabase.SaveAssets();

            #endif
        }

        #endregion
    }
}
