using Modules.Hive.Editor;
using Modules.Hive.Editor.BuildUtilities;
using System.IO;
using UnityEngine;


namespace Modules.BuildProcess
{
    public class TargetSettingsBuildProcess : IBuildPreprocessor<IBuildPreprocessorContext>
    {
        #region Fields

        private const string TargetSettingsFolderName = "TargetSettings";

        private const string SharedSettingsFolderName = "Shared";

        private const string ProfilesFolder = "Profiles";

        #endregion



        #region Methods

        public void OnPreprocessBuild(IBuildPreprocessorContext context)
        {
            string targetSettingsPath = 
                UnityPath.Combine(UnityPath.AssetsDirectoryName, TargetSettingsFolderName);
            
            string profilesPath =
                UnityPath.Combine(targetSettingsPath, ProfilesFolder);
            
            if (Directory.Exists(profilesPath))
            {
                ITargetSettingsActualizer targetSettingsActualizer;
                
                if (BuildInfo.IsChinaBuild)
                {
                    targetSettingsActualizer = new ConcreteTargetSettingsActualizer("China");
                }
                else
                {
                    targetSettingsActualizer = new ConcreteTargetSettingsActualizer("Common");   
                }

                if (!targetSettingsActualizer.TryActualizeSettingsAtPath(profilesPath, context))
                {
                    targetSettingsActualizer = new DefaultTargetSettingsActualizer();
                    targetSettingsActualizer.TryActualizeSettingsAtPath(profilesPath, context);
                }
            }

            if (Directory.Exists(targetSettingsPath))
            {
                ActualizeSharedSettings(targetSettingsPath, context);
            }
        }


        private void ActualizeSharedSettings(string targetSettingsPath, IBuildPreprocessorContext context)
        {
            string concreteSettingsFolderPath = 
                UnityPath.Combine(targetSettingsPath, SharedSettingsFolderName);

            if (Directory.Exists(concreteSettingsFolderPath))
            {
                ITargetSettingsActualizer sharedSettingsActualizeUtil 
                    = new SharedTargetSettingsActualizer(SharedSettingsFolderName);

                sharedSettingsActualizeUtil.TryActualizeSettingsAtPath(targetSettingsPath, context);
            }
        }

        #endregion
    }
}
