using System;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.LevelsRepository;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.Sound;


namespace Drawmasters.ServiceUtil
{
    public class MusicService : IMusicService
    {
        #region Fields

        private readonly ILevelEnvironment levelEnvironment;

        private string currentMusicKey;
        private Guid currentMusicGuid;

        #endregion



        #region Properties

        private string CurrentMusicKey
        {
            get
            {
                string musicKey;

                LevelContext context = levelEnvironment.Context;
                bool isBossLevel = levelEnvironment.Context.IsBossLevel;

                if (isBossLevel &&
                    !context.SceneMode.IsProposalSceneMode())
                {
                    musicKey = SoundGroupKeys.RandomBossLevelMusicKey;
                }
                else if (context.IsBonusLevel &&
                         !context.SceneMode.IsProposalSceneMode())
                {
                    musicKey = SoundGroupKeys.RandomBonusLevelMusicKey;
                }
                else if (context.SceneMode == GameMode.MenuScene)
                {
                    musicKey = AudioKeys.Music.MUSIC_MENU;
                }
                else
                {
                    musicKey = SoundGroupKeys.RandomLevelMusicKey;
                }

                return musicKey;
            }
        }

        #endregion



        #region Ctor

        public MusicService(ILevelEnvironment _levelEnvironment)
        {
            levelEnvironment = _levelEnvironment;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        #endregion



        #region IMusicService

        public void SetMusicVolume(float volume)
        {
            SoundManager.Instance.SetSoundVolume(currentMusicGuid, volume);
        }


        public void InstantRefreshMusic()
        {
            string musicKey = CurrentMusicKey;

            currentMusicGuid = SoundManager.Instance.PlaySound(musicKey, isLooping: true);
            currentMusicKey = musicKey;
        }

        #endregion



        #region Private methods

        private void RefreshMusic()
        {
            string musicKey = CurrentMusicKey;

            if (currentMusicKey != musicKey)
            {
                currentMusicGuid = SoundManager.Instance.PlaySound(musicKey, isLooping: true);
                currentMusicKey = musicKey;
            }
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state != LevelState.Unloaded)
            {
                RefreshMusic();
            }

            if (state == LevelState.Initialized &&
                levelEnvironment.Context.Mode.IsHitmastersLiveOps() &&
                levelEnvironment.Context.IsBossLevel)
            {
                GameServices.Instance.MusicService.InstantRefreshMusic();
            }
        }


        #endregion
    }
}

