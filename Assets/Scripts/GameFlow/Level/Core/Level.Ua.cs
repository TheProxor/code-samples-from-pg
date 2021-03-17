using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Levels
{
    public partial class Level
    {
        #region Methods

        public void ChangeBackground(string backgroundName)
        {
            int backgroundIndex = -1;

            if (int.TryParse(backgroundName, out backgroundIndex))
            {
                foreach (var levelObject in levelObjects)
                {
                    if (levelObject is BackgroundVisualObject backgroundObject)
                    {
                        backgroundObject.InitializeSprite(backgroundIndex);
                        backgroundObject.RefreshUaData();
                    }
                }
            }
        }


        public void ChangeBackgroundColor(Color color)
        {
            foreach (var levelObject in levelObjects)
            {
                if (levelObject is BackgroundVisualObject backgroundObject)
                {
                    backgroundObject.ChangeBackgroundColor(color, Vector2.one * 500f);
                    backgroundObject.RefreshUaData();
                }
            }
        }

        #endregion
    }
}
