namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface ILevelGraphicService
    {
        int GetLevelProfileIndex(GameMode mode, int chapterIndex, int levelIndex);

        bool IsLevelProfileExists(GameMode mode, int chapterIndex, int levelIndex);

        int RandomRewardProposalSceneIndex { get; }
    }
}
