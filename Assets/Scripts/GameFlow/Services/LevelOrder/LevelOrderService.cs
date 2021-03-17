
using System.Collections.Generic;
using System.Linq;
using Drawmasters.AbTesting;
using Drawmasters.Interfaces;
using Drawmasters.Levels.Order;
using Drawmasters.LevelsRepository;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.ServiceUtil
{
    public class LevelOrderService : ILevelOrderService
    {
        #region Fields

        public static readonly IUaAbTestMechanic uaAbTestMechanic =
            new CommonEnabledMechanicAvailability(PrefsKeys.Dev.DevAlternativeLevelsPackEnabled);

        private readonly ModeData[] modesData;
        private readonly Dictionary<GameMode, List<SublevelData>> modesLoadedData;
        private readonly OverflowMarker overflowingHelper;
        private readonly RandomLevelsGenerator randomLevelsHelper;

        private readonly IPlayerStatisticService playerStatistic;
        private readonly IAbTestService abTestService;

        #endregion



        #region Proeprties


        public bool ShouldLoadAlternativeLevelsPack =>
            uaAbTestMechanic.WasAvailabilityChanged ? 
                uaAbTestMechanic.IsMechanicAvailable : abTestService.CommonData.isUsedAlternativeLevels;

        #endregion



        #region Ctor

        public LevelOrderService(IPlayerStatisticService _playerStatistic, IAbTestService _abTestService)
        {
            abTestService = _abTestService;
            playerStatistic = _playerStatistic;

            LevelsOrder levelsOrder = IngameData.Settings.levelsOrder;
            LevelsOrder.AbTestReplaceData[] abTestReplaceData = abTestService.CommonData.abTestSublevelsReplaceData;

            modesData = ShouldLoadAlternativeLevelsPack ?
                levelsOrder.LoadAbTestModesData(abTestReplaceData) : levelsOrder.LoadModesData(abTestReplaceData);

            overflowingHelper = new OverflowMarker(modesData);
            randomLevelsHelper = new RandomLevelsGenerator();

            modesLoadedData = new Dictionary<GameMode, List<SublevelData>>();

            LoadActualData();
        }

        #endregion



        #region ILevelOrderService

        public SublevelData? FindData(GameMode mode, int index)
        {
            bool isOverflowed = overflowingHelper.IsModeOverflowed(mode, index);
            if (isOverflowed)
            {
                overflowingHelper.AddOverflow(mode, index);

                List<ChapterData> chaptersCopiedData = modesData.Find(m => m.mode == mode).Copy().chapters;

                randomLevelsHelper.RegenerateData(chaptersCopiedData, mode);

                LoadActualData();
            }

            return FindDataWithoutOverflowCheck(mode, index);
        }


        public SublevelData? FindData(GameMode mode, LevelType levelType, int chapterIndex)
        {
            SublevelData? result = null;

            if (modesLoadedData.TryGetValue(mode, out List<SublevelData> sublevels))
            {
                var chapterLevels = sublevels.Where(x => x.ChapterIndex == chapterIndex).ToList();

                foreach(var i in chapterLevels)
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
            SublevelData? data = default;
            
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
            int result = default;
            
            if (modesLoadedData != null &&
                modesLoadedData.TryGetValue(mode, out List<SublevelData> data))
            {
                result = data.Count;
            }

            return result;
        }

        #endregion



        #region Private methods

        private void LoadActualData()
        {
            modesLoadedData.Clear();

            GameMode[] modes = playerStatistic.ModesData.AllModes;

            for (int i = 0; i < modes.Length; i++)
            {
                GameMode mode = modes[i];

                List<ChapterData> chaptersData;
                List<string> uaLevels = new List<string>();
                
                bool wasModeOverflowed = overflowingHelper.WasModeOverflowed(mode);

                if (wasModeOverflowed)
                {
                    chaptersData = randomLevelsHelper.RandomedData(mode);
                }
                else
                {
                    ModeData modeData = modesData.Find(m => m.mode == mode);

                    if (modeData == null)
                    {
                        continue;
                    }

                    chaptersData = modeData.chapters;
                    uaLevels = modeData.specialUaLevels;
                }

                IAnyLoader<List<SublevelData>> dataLoader = 
                    new DataLoader(chaptersData, 
                                   overflowingHelper.GetOverflowsCount(mode), 
                                   uaLevels);

                dataLoader.Load();

                modesLoadedData.Add(mode, dataLoader.LoadedObject);
            }
        }        

        #endregion
    }
}

