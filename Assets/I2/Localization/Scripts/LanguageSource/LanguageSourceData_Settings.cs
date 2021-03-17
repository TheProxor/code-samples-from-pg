using System;
using UnityEngine;
using System.Linq;

namespace I2.Loc
{
	public partial class LanguageSourceData
	{
        #region Settings

        public string localizationFormatBehaviourType;
        
        public bool useInvariantCultureAsDefault;

        public ILocalizationFormatBehaviour LocalizationFormatBehaviour
        {
            get
            {
                ILocalizationFormatBehaviour result;

                Type behaviourType;

                if (string.IsNullOrEmpty(localizationFormatBehaviourType))
                {
                    SetDefaultType();
                }
                else
                {
                    behaviourType = Type.GetType(localizationFormatBehaviourType);
                }

                if (behaviourType == null)
                {
                    Debug.LogError($"Localization Format Behaviour Type {localizationFormatBehaviourType} is not exsists!");
                    SetDefaultType();
                }

                result = Activator.CreateInstance(behaviourType) as ILocalizationFormatBehaviour;

                return result;


                void SetDefaultType() => 
                    behaviourType = typeof(SafeStringFormatBehaviour);
            }
        }

        
        public string[] LocalizationFormatBehaviourTypes
        {
            get
            {
                Type type = typeof(ILocalizationFormatBehaviour);

                string[] types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && p != type)
                    .Select(x => x.FullName)
                    .ToArray();

                return types;
            }
        }


        public string[] FontTerms
        {
            get
            {
                return mTerms.FindAll(x => x.TermType == eTermType.TextMeshPFont).
                        Select(x => x.Term).
                        ToArray();
            }
        }

#endregion
    }
}