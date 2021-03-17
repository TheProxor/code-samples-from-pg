using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Drawmasters.Levels
{
    public static class Linker
    {
        #region Helpers

        [Serializable]
        public class Link
        {
            public int mainObject;
            public List<int> linkedObjects = new List<int>();
        }


        [Serializable]
        public class Data
        {
            public List<Link> links = new List<Link>();
        }

        #endregion



        #region Fields

        private static readonly Dictionary<LevelObjectData, List<LevelObjectData>> savedLinks = new Dictionary<LevelObjectData, List<LevelObjectData>>();

        #endregion



        #region Methods

        public static void SetLinks(List<LevelObject> levelObjects, Data data)
        {
            if(savedLinks.Count > 0)
            {
                SetSavedLinks(levelObjects);
                return;
            }

            foreach (var link in data.links)
            {
                List<LevelObject> linkedObjects = new List<LevelObject>(link.linkedObjects.Count);

                foreach (var index in link.linkedObjects)
                {
                    linkedObjects.Add(levelObjects[index]); 
                }

                levelObjects[link.mainObject].SetLinks(linkedObjects);


                List<LevelObjectData> datas = linkedObjects.Select(x => x.CurrentData).ToList();

                savedLinks.Add(levelObjects[link.mainObject].CurrentData, datas);
            }
        }


        public static void ClearSavedLinks()
        {
            savedLinks.Clear();
        }


        private static void SetSavedLinks(IReadOnlyList<LevelObject> levelObjects)
        {
            foreach (var item in levelObjects)
            {
                if (savedLinks.ContainsKey(item.CurrentData))
                {
                    var linkedObjects = levelObjects.Where(x => savedLinks[item.CurrentData].Contains(x.CurrentData)).ToList();
                    item.SetLinks(linkedObjects);
                }
            }
        }

        #endregion
    }
}

