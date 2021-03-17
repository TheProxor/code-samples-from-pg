using UnityEngine;
using System;


namespace Drawmasters.Utils
{
    /// <summary>
    /// Inherit from this class to allow polymorphic (de)serialisation via JsonUtility. 
    /// Inheriting classes should be marked as [System.Serializable]
    /// </summary>
    public abstract class PolymorphicObject
    {
        [HideInInspector]
        public string AssemblyQualifiedName; // for deserialising as the correct type

        public static T FromJson<T>(string json) where T : PolymorphicObject
        {
            // deserialise first as PolymorphicObject to get the instance Type
            var type = Type.GetType(JsonUtility.FromJson<T>(json).AssemblyQualifiedName);

            // deserialise as the correct type
            return (T)JsonUtility.FromJson(json, type);
        }

        public PolymorphicObject()
        {
            // AssemblyQualifiedName is public and will be serialised
            // when using JsonUtility.ToJson
            RefreshAssemblyQualifiedName();
        }


        public void RefreshAssemblyQualifiedName()
        {
            var type = this.GetType();
            AssemblyQualifiedName = type.AssemblyQualifiedName;
        }
    }
}
