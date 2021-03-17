using UnityEngine;
using System;
using System.Linq;

namespace Drawmasters.Editor
{
    public class ValidatorPrefsKeys : ValidatorTypes
    {
        #region Overrided properties

        protected override Type RootType => typeof(PrefsKeys);

        #endregion



        #region Protected methods

        protected override bool ValidateType()
        {
            bool result = true;

            string[] keys = RootType.GetAllFieldsKeys<string>();

            foreach (var group in keys.GroupBy(x => x))
            {
                if (group.Count() > 1)
                {
                    Debug.Log($"Key <b>{group.Key} already</b> exsists in {nameof(PrefsKeys)}; Detected <b>{group.Count() - 1}</b> repeats.");
                    result = false;
                }
            }
            
            return result;
        }

        #endregion
    }
}
