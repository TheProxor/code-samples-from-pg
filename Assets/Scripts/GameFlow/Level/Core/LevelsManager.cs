using Drawmasters.Levels.Helpers;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LevelsManager : SingletonMonoBehaviour<LevelsManager>, IInitializable
    {
        #region Fields

        [SerializeField] private Level level = default;

        #endregion



        #region Properties

        public Level Level => level;

        #endregion



        #region IInitializable

        public void Initialize()
        {
            level.Initialize();
        }

        #endregion



        #region Methods

        public void UnloadLevel()
        {
            level.OnLevelEnd -= Level_OnLevelEnd;
            level.UnloadLevel();
        }

        public void LoadLevel(GameMode gameMode, int levelIndex)
        {
            GameModesInfo.TryConvertModeToWeapon(gameMode, out WeaponType weapon);

            level.LoadLevel(gameMode, levelIndex, false, weapon, GameMode.None);
            level.OnLevelEnd += Level_OnLevelEnd;
        }

        public void ReloadLevel()
        {
            level.PlayLevel(true);
        }


        public void PlayLevel()
        {
            level.PlayLevel();
        }


        public void LoadScene(GameMode gameMode, GameMode specifiedMode)
        {
            //TODO check
            GameModesInfo.TryConvertModeToWeapon(gameMode, out WeaponType weapon);

            //TODO skin hotfix
            if (specifiedMode == GameMode.MenuScene)
            {
                weapon = WeaponType.Sniper;                
            }

            int levelIndex = gameMode.GetCurrentLevelIndex();

            level.LoadLevel(gameMode, levelIndex, false, weapon, specifiedMode);
            level.OnLevelEnd += Level_OnLevelEnd;
        }


        public void ClearLevel()
        {
            level.OnLevelEnd -= Level_OnLevelEnd;
            level.FinishPlayLevel();
        }


        public void CompleteLevel(LevelResult resultState) => 
            FinishLevel(resultState);


        private void FinishLevel(LevelResult resultState)
        {
            bool canFinish = level.CurrentState.CanFinish(resultState);
            if (!canFinish)
            {
                // that can happen if we will end level and get deferred callback for interstitial showed
                CustomDebug.Log($"Trying to finish already finished level");
                return;
            }

            level.OnLevelEnd -= Level_OnLevelEnd;

            LevelProgressObserver.TriggerLevelStateChanged(resultState);
            level.ChangeState(LevelState.EndPlaying);

            ILevelFinisher finisher = default;
            finisher = finisher.DefineFinisher();
            finisher.FinishLevel(() => level.FinishPlayLevel());
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelEnd(LevelResult levelResult) =>
            FinishLevel(levelResult);

        #endregion



        #region Editor code
        #if UNITY_EDITOR
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (level.CurrentState == LevelState.Playing)
                {
                    CompleteLevel(LevelResult.Complete);
                }
            }
        }

        #endif
        #endregion
    }
}
