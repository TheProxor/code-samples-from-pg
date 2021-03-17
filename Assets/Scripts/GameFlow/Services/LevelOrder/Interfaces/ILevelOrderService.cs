using Drawmasters.Levels.Order;

namespace Drawmasters.ServiceUtil
{
    public interface ILevelOrderService
    {
        bool ShouldLoadAlternativeLevelsPack { get; }

        SublevelData? FindData(GameMode mode, int index);

        SublevelData? FindData(GameMode mode, LevelType levelType, int chapterIndex);

        SublevelData? FindDataWithoutOverflowCheck(GameMode mode, int index);

        int FindLevelsCount(GameMode mode);
    }
}
