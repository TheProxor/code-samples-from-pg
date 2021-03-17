using Drawmasters.Levels;
using System.Collections.Generic;


namespace Drawmasters.LevelConstructor
{
    public static class EditorLinker
    {
        #region Fields

        static Dictionary<EditorLevelObject, List<EditorLevelObject>> links = new Dictionary<EditorLevelObject, List<EditorLevelObject>>();

        #endregion



        #region Methods

        public static void Init(Linker.Data data, List<EditorLevelObject> levelObjects)
        {
            Clear();

            foreach (var link in data.links)
            {
                List<EditorLevelObject> linkedObjects = new List<EditorLevelObject>(link.linkedObjects.Count);

                foreach (var index in link.linkedObjects)
                {
                    linkedObjects.Add(levelObjects[index]); 
                }

                links.Add(levelObjects[link.mainObject], linkedObjects);
            }
        }


        public static Linker.Data GetSerializableLinks(List<EditorLevelObject> levelObjects)
        {
            Linker.Data data = new Linker.Data();

            foreach (var editorLink in links)
            {
                Linker.Link link = new Linker.Link();

                link.mainObject = levelObjects.IndexOf(editorLink.Key);

                foreach (var levelObject in editorLink.Value)
                {
                    link.linkedObjects.Add(levelObjects.IndexOf(levelObject)); 
                }

                data.links.Add(link);
            }

            return data;
        }


        public static List<EditorLevelObject> GetLinks(EditorLevelObject levelObject)
        {
            return links.ContainsKey(levelObject) ? links[levelObject] : new List<EditorLevelObject>();
        }


        public static void Clear() => links.Clear();


        public static void AddLink(EditorLevelObject mainObject, EditorLevelObject linkedObject)
        {
            if (mainObject != linkedObject)
            {
                if (links.ContainsKey(mainObject))
                {
                    if (!mainObject.CanHaveMultipleLinkedObjects)
                    {
                        links[mainObject].Clear();
                    }

                    links[mainObject].AddExclusive(linkedObject);
                }
                else
                {
                    links.Add(mainObject, new List<EditorLevelObject> { linkedObject });
                }
            }
        }


        public static void RemoveObject(EditorLevelObject levelObject)
        {
            links.Remove(levelObject);

            List<EditorLevelObject> keysToRemove = new List<EditorLevelObject>();

            foreach (var pair in links)
            {
                if (pair.Value.Contains(levelObject))
                {
                    pair.Value.Remove(levelObject);

                    if (pair.Value.Count == 0)
                    {
                        keysToRemove.Add(pair.Key); 
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                links.Remove(key); 
            }
        }


        public static void RemoveLink(EditorLevelObject mainObject, EditorLevelObject linkedObject)
        {
            links[mainObject].Remove(linkedObject);
        }

        #endregion
    }
}
