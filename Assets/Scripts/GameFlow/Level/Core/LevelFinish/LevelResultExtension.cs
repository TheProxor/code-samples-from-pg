namespace Drawmasters.Levels
{
    public static class LevelResultExtension
    {
        #region Methods

        public static bool IsLevelOrProposalAccomplished(this LevelResult levelResult) =>
            IsLevelAccomplished(levelResult) ||
            IsProposalAccomplished(levelResult);

        public static bool IsLevelAccomplished(this LevelResult levelResult) =>
            IsCompleted(levelResult) ||
            IsSkipped(levelResult);

        public static bool IsCompleted(this LevelResult levelResult) =>
            levelResult == LevelResult.Complete;

        public static bool IsSkipped(this LevelResult levelResult) =>
            levelResult == LevelResult.IngameSkip || levelResult == LevelResult.ResultSkip;

        public static bool IsProposalAccomplished(this LevelResult levelResult) =>
            levelResult == LevelResult.ProposalEnd;

        #endregion
    }
}

