using System;


namespace Drawmasters.Statistics
{
    [Serializable]
    public class AttemptData
    {
        public GameMode mode = default;
        public int index = default;
        public int attempt = default;


        public AttemptData(GameMode _mode, int _index, int _attempt)
        {
            mode = _mode;
            index = _index;
            attempt = _attempt;
        }

        public AttemptData() { }
    }
}

