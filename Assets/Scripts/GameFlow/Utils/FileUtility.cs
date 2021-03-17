using System;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Drawmasters.Utils
{
    public static class FileUtility
    {
        public static void ReadFileByLines(Action<string> onLineRead, string path = default, int skipLines = 0)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                #if UNITY_EDITOR
                      path = EditorUtility.OpenFilePanel("Select File", "Assets", string.Empty);
                #endif
            }

            using (FileStream fs = File.OpenRead(path))
            using (StreamReader tr = new StreamReader(fs))
            {
                for (int i = 0; i < skipLines; i++)
                {
                    tr.ReadLine();
                }

                while (!tr.EndOfStream)
                {
                    string line = tr.ReadLine();

                    onLineRead?.Invoke(line);
                }
            }

            Debug.Log("Done import common data from file: " + path);
        }
    }
}
