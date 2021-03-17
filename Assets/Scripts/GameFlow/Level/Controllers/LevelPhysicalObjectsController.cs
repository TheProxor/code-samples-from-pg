using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class LevelPhysicalObjectsController : ILevelController
    {
        #region Fields

        private readonly List<PhysicalLevelObject> activeObjects = new List<PhysicalLevelObject>();

        #endregion
        
        
        
        #region Properties

        public List<PhysicalLevelObject> AllObjects => activeObjects;
        
        #endregion



        #region Methods

        public void Initialize() { }

        public void Deinitialize()
        {
            for (int i = activeObjects.Count - 1; i > -1; i--)
            {
                Remove(i);
            }

            if (activeObjects.Count != 0)
            {
                CustomDebug.Log("List isn't empty");
            }
        }


        public void Add(PhysicalLevelObject physicalObject)
        {
            physicalObject.OnDestroy += PhysicalObject_OnDestroy;
            activeObjects.Add(physicalObject);
        }


        private void Remove(int index)
        {
            if (index >= activeObjects.Count || index < 0)
            {
                return;
            }

            activeObjects[index].OnDestroy -= PhysicalObject_OnDestroy;
            activeObjects[index].FinishGame();
            activeObjects.RemoveAt(index);
        }

        #endregion



        #region Events handlers

        private void PhysicalObject_OnDestroy(PhysicalLevelObject physicalObject)
        {
            int foundIndex = activeObjects.FindIndex(t => t.Equals(physicalObject));
            
            Remove(foundIndex);
        }

        #endregion
    }
}
