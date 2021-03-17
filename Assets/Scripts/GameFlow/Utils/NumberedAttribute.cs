using UnityEngine;


namespace Drawmasters
{
    public class NumberedAttribute : PropertyAttribute
    {
        #region Properties

        public string ElementName { get; private set; }

        #endregion



        #region Ctor

        public NumberedAttribute() { }


        public NumberedAttribute(string _elementName) { ElementName = _elementName; }

        #endregion
    }
}
