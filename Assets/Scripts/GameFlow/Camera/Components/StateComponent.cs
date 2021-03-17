using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;


namespace Drawmasters.CameraUtil
{
    public class StateComponent : IInitializable, IDeinitializable
    {
        #region Fields

        private readonly Camera mainCamera;
        private readonly float defaultSize;
        private readonly ILevelEnvironment levelEnvironment;

        #endregion



        #region Ctor

        public StateComponent(Camera camera, float _defaultSize)
        {
            mainCamera = camera;

            defaultSize = _defaultSize;

            levelEnvironment = GameServices.Instance.LevelEnvironment;
        }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
        }

        #endregion



        #region Private methods

        private void SetupMenuSettings()
        {
            if (!IngameCamera.IsSizeLocked)
            {
                mainCamera.orthographicSize = 0.5f * defaultSize;
            }

            mainCamera.transform.localPosition = mainCamera.transform.localPosition.SetY(-16f);
        }


        private void SetupIngameSettings()
        {
            if (!IngameCamera.IsSizeLocked)
            {
                mainCamera.orthographicSize = defaultSize;
            }

            mainCamera.transform.localPosition = mainCamera.transform.localPosition.SetY(0f);
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.Initialized)
            {
                if (levelEnvironment.Context.SceneMode == GameMode.MenuScene)
                {
                    SetupMenuSettings();
                }
                else
                {
                    SetupIngameSettings();
                }
            }
        }

        #endregion
    }
}

