using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEditor.Build.Reporting;


namespace Drawmasters.Editor
{
    internal class PreProcessBuildValidate : IPreprocessBuildWithReport
    {
        #region Properties

        private static IValidator[] Validators => new IValidator[]
            {
                new ValidatorLevelAssets(),
                new ValidatorEffectAssets(),
                new ValidatorSoundAssets(),
                new ValidatorIAPsAssets(),
                new ValidatorPrefsKeys()
            };

        #endregion
        
        
        #region Public Methods

        [MenuItem("Drawmasters/Validate")]
        public static void MenuValidateAll()
        {
            ValidateAll(Validators);
        }

        #endregion



        #region Private Methods

        private static bool ValidateAll(IValidator[] validators)
        {
            bool result = true;

            foreach (var validator in validators)
            {
                if (validator.Validate)
                {
                    Debug.Log(validator.SuccessfulValidateMessage);
                }
                else
                {
                    Debug.LogError(validator.FailValidateMessage);
                    result = false;
                }
            }

            return result;
        }
        
        #endregion



        #region IPreprocessBuildWithReport

        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!ValidateAll(Validators))
            {
                throw new BuildFailedException("CheckAssets fail");    
            }
        }

        #endregion
    }
}
