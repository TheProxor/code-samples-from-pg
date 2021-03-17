using System;

namespace Drawmasters.Editor
{
    public abstract class ValidatorTypes : IValidator
    {
        #region Properties

        public bool Validate => ValidateType();

        public string SuccessfulValidateMessage => $"Type '<b>{RootType.Name}</b>' validation succeed";

        public string FailValidateMessage => $"Type '<b>{RootType.Name}</b>' validation failed";



        protected abstract Type RootType { get; }

        #endregion


        #region Abstract Methods

        protected abstract bool ValidateType();       

        #endregion
    }
}
