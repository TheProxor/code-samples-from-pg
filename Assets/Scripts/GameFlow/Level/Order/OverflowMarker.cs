using System.Collections.Generic;


namespace Drawmasters.Levels.Order
{
    public class OverflowMarker
    {
        #region Fields

        private readonly IReadOnlyList<ModeData> modesData;
        
        private readonly Dictionary<GameMode, int> overflowsDict;

        #endregion



        #region Ctor

        public OverflowMarker(ModeData[] _modesData)
        {
            modesData = _modesData;

            if (CustomPlayerPrefs.HasKey(PrefsKeys.PlayerInfo.HoldedLevels))
            {
                overflowsDict = CustomPlayerPrefs.GetObjectValue<Dictionary<GameMode, int>>(PrefsKeys.PlayerInfo.HoldedLevels);
            }
            else
            {
                overflowsDict = new Dictionary<GameMode, int>();

                foreach (var modeData in modesData)
                {
                    overflowsDict.Add(modeData.mode, default);
                }
            }
        }

        #endregion



        #region Api

        public void AddOverflow(GameMode mode, int index)
        {
            int overflows = CurrentOverflowsCount(mode, index);

            AddElement(mode, overflows);
        }


        public bool WasModeOverflowed(GameMode mode)
            => GetElement(mode) > 0;


        public bool IsModeOverflowed(GameMode mode, int index)
        {
            int current = CurrentOverflowsCount(mode, index);
            int holded = GetElement(mode);


            return current > holded;
        }

        public int GetOverflowsCount(GameMode mode)
            => GetElement(mode);
        
        #endregion


        
        #region Private methods
        
        private int CurrentOverflowsCount(GameMode mode, int index)
            => SublevelsCount(mode) == 0 ? 0 : index / SublevelsCount(mode);       


        private int SublevelsCount(GameMode gameMode)
        {
            foreach(var m in modesData)
            {
                if (m.mode == gameMode)
                {
                    return m.SublevelsCount;
                }
            }

            return default;
        }

        #endregion



        #region Dictionary Api

        private int GetElement(GameMode mode)
        {
            int count = default;

            if (overflowsDict.TryGetValue(mode, out int index))
            {
                count = index;
            }
            else
            {
                CustomDebug.Log("Missing key in dict. Mode : " + mode);
            }

            return count;
        }


        private void AddElement(GameMode mode, int index)
        {
            if (overflowsDict.TryGetValue(mode, out int holdedIndex))
            {
                overflowsDict[mode] = index;
            }
            else
            {
                overflowsDict.Add(mode, index);
            }

            CustomPlayerPrefs.SetObjectValue(PrefsKeys.PlayerInfo.HoldedLevels, overflowsDict);
        }

        #endregion
    }
}

