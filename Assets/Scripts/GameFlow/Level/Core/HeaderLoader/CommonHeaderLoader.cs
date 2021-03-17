using System;
using Drawmasters.Levels.Order;
using Drawmasters.LevelsRepository;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public class CommonHeaderLoader : IHeaderLoader
    {
        public LevelHeader LoadedHeader { get; private set; }


        public LevelHeader LoadHeader(GameMode mode, int index, GameMode specialScene, WeaponType weaponType)
        {
            ILevelOrderService levelOrderService = LevelsOrderServiceSelector.Select(mode);

            LoadedHeader = LevelsContainer.GetSublevelHeader(levelOrderService, mode, index);

            return LoadedHeader;
        }
    }

    public class AbTestHeaderLoader : IHeaderLoader 
    {
        public LevelHeader LoadedHeader { get; private set; }

        public LevelHeader LoadHeader(GameMode mode, int index, GameMode specialScene, WeaponType weaponType)
        {
            ILevelOrderService levelOrderService = LevelsOrderServiceSelector.Select(mode);
            SublevelData? data = levelOrderService.FindData(mode, index);

            if (data != null)
            {
                LevelType type = LevelsContainer.GetLevelType(mode, data.Value.NameId);

                if (type == LevelType.Boss)
                {
                    SublevelData? bonusData = levelOrderService.FindData(mode, LevelType.Bonus, data.Value.ChapterIndex);
                    if (bonusData != null)
                    {
                        LoadedHeader = LevelsContainer.GetHeader(mode, bonusData.Value.NameId);
                    }
                    else
                    {
                        CustomDebug.Log($"Missing header loader. Mode = {mode}, level type = {LevelType.Bonus}, chapter index = {data.Value.ChapterIndex}");
                    }
                }
                else
                {
                    LoadedHeader = LevelsContainer.GetSublevelHeader(levelOrderService, mode, index);
                }
            }

            return LoadedHeader;
        }
    }
}