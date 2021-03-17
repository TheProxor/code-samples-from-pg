using Modules.Hive.Editor;
using Modules.Hive.Editor.BuildUtilities;
using System.Collections.Generic;
using System.IO;
using UnityEditor;


namespace Modules.BuildProcess
{
    public class DefaultTargetSettingsActualizer : ITargetSettingsActualizer
    {
        #region Fields

        private static readonly Dictionary<BuildTarget, string> ProjectSettingsPathsByTarget = new Dictionary<BuildTarget, string>()
        {
            { BuildTarget.Android, "Android" },
            { BuildTarget.iOS, "iOS" }
        };

        #endregion



        #region Methods

        public virtual bool TryActualizeSettingsAtPath(string settingsPath, IBuildPreprocessorContext context)
        {
            if (Directory.Exists(settingsPath))
            {
                foreach (var pair in ProjectSettingsPathsByTarget)
                {
                    string platformSettingsFolderName =
                        UnityPath.Combine(settingsPath, pair.Value);
                    if (pair.Key == context.BuildTarget)
                    {
                        ProjectSnapshot.CurrentSnapshot?.CopyAssetToStash(platformSettingsFolderName);
                    }
                    else
                    {
                        ProjectSnapshot.CurrentSnapshot?.MoveAssetToStash(platformSettingsFolderName);
                    }
                }

                string globalSettingsFolderName =
                    UnityPath.Combine(settingsPath, UnityPath.ResourcesDirectoryName);

                if (Directory.Exists(globalSettingsFolderName))
                {
                    ProjectSnapshot.CurrentSnapshot?.CopyAssetToStash(globalSettingsFolderName);
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}
