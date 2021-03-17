using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;


namespace Drawmasters.Effects
{
    public partial class EffectManager
    {
        #if UNITY_EDITOR

        #region Fields

        private const string AUDIO_KEYS_PATH = "Assets/Scripts/GameFlow/EffectSystem/EffectKeys.cs";
        private const string RESOURCES_SOUNDS_DIRECTORY = "Assets/Resources/Effects";

        #endregion



        #region Methods
        
        [Sirenix.OdinInspector.Button]
        private void RefreshEffects()
        {
            systemHandlersAssetsLink = GetEffects();

            if (File.Exists(AUDIO_KEYS_PATH))
            {
                File.Delete(AUDIO_KEYS_PATH);
            }

            StreamWriter outfile = new StreamWriter(AUDIO_KEYS_PATH);

            outfile.WriteLine("public class EffectKeys");
            outfile.WriteLine("{");

                outfile.WriteLine("public const string None = \"\";", "None", "\"\"");

                PerformActionOnFilesRecursively(RESOURCES_SOUNDS_DIRECTORY, fileName =>
                {
                    EffectHandler config = AssetDatabase.LoadAssetAtPath<EffectHandler>(fileName);

                    if (config != null)
                    {
                        AssetLink configLink = new AssetLink(config);
                        string fileCapitalName = configLink.Name;
                        fileCapitalName.Replace('-', '_');
                        string soundName = string.Format("\"{0}\"", configLink.Name);

                        string resultString = string.Format("public const string {0} = {1};", fileCapitalName, soundName);

                        outfile.WriteLine(resultString);
                    }
                });
                outfile.WriteLine("}");

            outfile.Close();

            AssetDatabase.Refresh();
            CustomDebug.Log($"<color=green> Effects were refreshed </color>");
        }


        List<AssetLink> GetEffects()
        {
            List<AssetLink> result = new List<AssetLink>();

            if (Directory.Exists(RESOURCES_SOUNDS_DIRECTORY))
            {
                PerformActionOnFilesRecursively(RESOURCES_SOUNDS_DIRECTORY, fileName =>
                {
                    EffectHandler loadedHandler = AssetDatabase.LoadAssetAtPath<EffectHandler>(fileName);

                    if (loadedHandler != null)
                    {
                        var assetLink = new AssetLink(loadedHandler.gameObject);
                        result.Add(assetLink);
                    }
                });
            }

            return result;
        }


        private void PerformActionOnFilesRecursively(string rootDir, Action<string> action)
        {
            foreach (string dir in Directory.GetDirectories(rootDir))
            {
                PerformActionOnFilesRecursively(dir, action);
            }

            foreach (string file in Directory.GetFiles(rootDir))
            {
                action(file);
            }
        }

        #endregion

        #endif
    }
}
