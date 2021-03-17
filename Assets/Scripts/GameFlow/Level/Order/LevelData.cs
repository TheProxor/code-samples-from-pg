using System;
using System.Collections.Generic;

namespace Drawmasters.Levels.Order
{
    [Serializable]
    public class LevelData
    {
        [Numbered]
        public List<string> sublevels = default;

        public LevelData Copy() => new LevelData { sublevels = new List<string>(sublevels) };
    }
}