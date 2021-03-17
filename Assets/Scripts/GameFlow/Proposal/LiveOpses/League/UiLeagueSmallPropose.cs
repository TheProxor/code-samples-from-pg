using System;
using Modules.Sound;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiLeagueSmallPropose : UiLeagueBasePropose
    {
        #region Properties

        public override bool ShouldShowProposalRoot =>
            base.ShouldShowProposalRoot &&
            controller.IsActive;

        #endregion
    }
}
