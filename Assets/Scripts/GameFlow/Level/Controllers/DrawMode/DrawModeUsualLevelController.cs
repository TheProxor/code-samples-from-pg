using Drawmasters.Levels.Data;
using Drawmasters.LevelsRepository;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class DrawModeUsualLevelController : DrawModeLevelController
    {
        #region Ctor

        public DrawModeUsualLevelController(ILevelEnvironment _levelEnvironment)
            : base(_levelEnvironment)
        {
        }

        #endregion



        #region Abstract implementation

        protected override bool IsControllerEnabled
        {
            get
            {
                bool result = base.IsControllerEnabled;

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;

                if (context != null)
                {
                    result &= !context.IsBonusLevel;
                    result &= !context.IsBossLevel;
                }

                return result;
            }
        }


        protected override void ShootersInputLevelController_OnStartDraw(Shooter shooter, Vector2 position)
        {
            InvokeLevelStateChangedEvent(LevelState.ReturnToInitial);

            if (!levelEnvironment.Context.IsBossLevel)
            {
                List<LevelObject> levelObjectsToClear =  Level.levelObjects.Where(e => e.IsHardReturnToInitialState).ToList();

                Level.ClearObjects(levelObjectsToClear);

                Level.Data levelData = LevelsContainer.GetLevelData(Level.headerLoader.LoadedHeader);
                List<LevelObjectData> dataToAdd = new List<LevelObjectData>(levelData.levelObjectsData);
                dataToAdd = dataToAdd.Where(e => Content.Management.FindLevelObject(e.index).IsHardReturnToInitialState).ToList();

                List<LevelObject> createdObjects = new List<LevelObject>();

                foreach (var data in dataToAdd)
                {
                    LevelObject createdObject = Level.CreateObject(data);
                    createdObjects.Add(createdObject);
                }

                Physics2D.SyncTransforms();
                Linker.SetLinks(Level.levelObjects, levelData.linkerData);
                Physics2D.SyncTransforms();

                createdObjects.ForEach(o => o.StartGame(levelEnvironment.Context.Mode,
                                                        levelEnvironment.Context.WeaponType,
                                                        Level.transform));
            }

            foreach (var levelObject in Level.levelObjects)
            {
                if (!levelObject.IsHardReturnToInitialState)
                {
                    levelObject.ReturnToInitialState();
                }
            }

            GameServices.Instance.LevelControllerService.ReturnToInitialState();

            InvokeLevelStateChangedEvent(LevelState.Playing);
        }

        #endregion
    }
}
