namespace Drawmasters.Editor
{
    public abstract class ValidatorAssets : IValidator
    {
        #region Properties

        public bool Validate => ValidateAsset(AssetName);

        public string SuccessfulValidateMessage => $"Asset '<b>{AssetName}</b>' validation succeed";

        public string FailValidateMessage => $"Asset '<b>{AssetName}</b>' validation failed";


        protected abstract string AssetName { get; }
        #endregion


        #region Abstract Methods

        protected abstract bool ValidateAsset(string name);

        #endregion
    }
}
