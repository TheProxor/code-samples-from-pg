using System;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public partial class Level 
    {
        #region Nested types

        [Serializable]
        public class Data
        {
            #region Fields

            public List<LevelObjectData> levelObjectsData = default;
            public Linker.Data linkerData = default;
            public float pathDistance = default;

            #endregion



            #region Class lifecycle

            public Data()
            {
                levelObjectsData = new List<LevelObjectData>();
                linkerData = new Linker.Data();
            }

            #endregion


            [Sirenix.OdinInspector.Button]
            private void Refresh()
            {
                foreach (var i in levelObjectsData)
                {
                    if (i.additionalInfo.Contains($"\"CurrencyType\":2,\"CurrencyCount\":10.0}}}}"))
                    {
                        i.additionalInfo = i.additionalInfo.Replace($"\"CurrencyType\":2,\"CurrencyCount\":10.0}}}}", $"\"CurrencyType\":2,\"CurrencyCount\":10.0}}");
                    }
                }
            }
        }


        #endregion
    }
}
