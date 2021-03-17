using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.AbTesting;
using Drawmasters.Interfaces;
using Drawmasters.Levels.Order;
using Drawmasters.LevelsRepository;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.ServiceUtil
{
    public class HitmastersLiveOpsLevelOrderService : ILevelOrderService
    {
        #region Fields

        public static IUaAbTestMechanic uaAbTestMechanic = new CommonMechanicAvailability(PrefsKeys.Dev.DevAlternativeLevelsPackEnabled);

        private readonly ModeData[] modesData;
        private readonly Dictionary<GameMode, List<SublevelData>> modesLoadedData;

        private readonly IPlayerStatisticService playerStatistic;
        private readonly IAbTestService abTestService;

        #endregion



        #region Properties

        // TODO: future implementation
        public bool ShouldLoadAlternativeLevelsPack =>
            uaAbTestMechanic.WasAvailabilityChanged ? uaAbTestMechanic.IsMechanicAvailable : true;

        #endregion



        #region Ctor

        public HitmastersLiveOpsLevelOrderService(LevelsOrder levelsOrder, IPlayerStatisticService _playerStatistic, IAbTestService _abTestService)
        {
            abTestService = _abTestService;
            playerStatistic = _playerStatistic;

            LevelsOrder.AbTestReplaceData[] abTestReplaceData = Array.Empty<LevelsOrder.AbTestReplaceData>();

            modesData = ShouldLoadAlternativeLevelsPack ?
                levelsOrder.LoadAbTestModesData(abTestReplaceData) : levelsOrder.LoadModesData(abTestReplaceData);

            modesLoadedData = new Dictionary<GameMode, List<SublevelData>>();

            LoadActualData();
        }

        #endregion



        #region ILevelOrderService

        public SublevelData? FindData(GameMode mode, int index) =>
            FindDataWithoutOverflowCheck(mode, index);


        public SublevelData? FindData(GameMode mode, LevelType levelType, int chapterIndex)
        {
            SublevelData? result = null;

            if (modesLoadedData.TryGetValue(mode, out List<SublevelData> sublevels))
            {
                var chapterLevels = sublevels.Where(x => x.ChapterIndex == chapterIndex).ToList();

                foreach (var i in chapterLevels)
                {
                    LevelType type = LevelsContainer.GetLevelType(mode, i.NameId);

                    if (type == levelType)
                    {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        }

        public SublevelData? FindDataWithoutOverflowCheck(GameMode mode, int index)
        {
            SublevelData data = default;

            if (modesLoadedData.TryGetValue(mode, out List<SublevelData> sublevels))
            {
                int clampedIndex = index % sublevels.Count;

                data = sublevels[clampedIndex];
            }
            else
            {
                CustomDebug.Log($"Missing data. Mode = {mode}");
            }

            return data;
        }

        public int FindLevelsCount(GameMode mode)
        {
            int count = 0;
            
            if (modesLoadedData != null &&
                modesLoadedData.TryGetValue(mode, out List<SublevelData> data))
            {
                count = data.Count;
            }

            return count;
        }

        #endregion



        #region Methods

        public bool IsIndexOverflow(GameMode mode, int index)
        {
            bool result = default;

            if (modesLoadedData.TryGetValue(mode, out List<SublevelData> sublevels))
            {
                result = index >= sublevels.Count;
            }
            else
            {
                CustomDebug.Log($"Missing data. Mode = {mode}");
            }

            return result;
        }


        private void LoadActualData()
        {
            modesLoadedData.Clear();

            GameMode[] modes = playerStatistic.ModesData.AllModes;

            for (int i = 0; i < modes.Length; i++)
            {
                GameMode mode = modes[i];

                List<ChapterData> chaptersData;


                ModeData modeData = modesData.Find(m => m.mode == mode);

                if (modeData == null)
                {
                    continue;
                }

                chaptersData = modeData.chapters;

                IAnyLoader<List<SublevelData>> dataLoader = new DataLoader(chaptersData, 0, new List<string>());

                dataLoader.Load();

                modesLoadedData.Add(mode, dataLoader.LoadedObject);
            }
        }

        #endregion
    }
}
