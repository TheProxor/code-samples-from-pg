using Drawmasters.Proposal.Interfaces;


namespace Drawmasters.Proposal
{
    public class UiPanelRewardController : IShowsCount
    {
        #region Fields

        private readonly string showCountsKey;

        #endregion



        #region Properties

        public AdsSkinPanelsSettings Settings { get; }

        #endregion



        #region Class lifecycle

        public UiPanelRewardController(AdsSkinPanelsSettings _settings, string _showCountsKey)
        {
            Settings = _settings;
            showCountsKey = _showCountsKey;
        }

        #endregion



        #region IShowsCount

        public int ShowsCount
        {
            get => CustomPlayerPrefs.GetInt(showCountsKey);
            set => CustomPlayerPrefs.SetInt(showCountsKey, value);
        }

        #endregion
    }
}
