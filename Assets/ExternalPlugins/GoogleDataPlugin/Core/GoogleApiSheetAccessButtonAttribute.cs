using System;


namespace Google.ApiPlugin
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class GoogleApiSheetAccessButtonAttribute : Attribute
    {
        public string Name { get; } = default;

        public GoogleApiSheetAccessButtonAttribute()
        {
        }


        public GoogleApiSheetAccessButtonAttribute(string _name)
        {
            Name = _name;
        }
    }
}

