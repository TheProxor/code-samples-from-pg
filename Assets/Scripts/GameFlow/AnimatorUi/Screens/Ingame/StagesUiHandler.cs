using System.Collections.Generic;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class StagesUiHandler : MonoBehaviour
    {
        #region Fields

        private readonly List<StageUi> stages = new List<StageUi>();

        [SerializeField] private StageUi stageUiPrefab = default;
        [SerializeField] private RectTransform stageUiRoot = default;

        private ILevelEnvironment levelEnvironment;

        #endregion


        #region Public methods

        public void Initialize()
        {
            levelEnvironment = GameServices.Instance.LevelEnvironment;

            bool needShowStages = !levelEnvironment.Context.IsBossLevel;
            needShowStages &= IngameScreen.IsUiEnabled;

            CommonUtility.SetObjectActive(stageUiRoot.gameObject, needShowStages);

            for (int i = 0; i < levelEnvironment.Context.SublevelsCount; i++)
            {
                StageUi stage = Instantiate(stageUiPrefab, stageUiRoot);
                bool isCompleted = i < levelEnvironment.Context.SublevelIndex;
                stage.Initialize(isCompleted);

                stages.Add(stage);
            }

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        public void Deinitialize()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            foreach (var stage in stages)
            {
                stage.Deinitialize();
                Destroy(stage.gameObject);
            }

            stages.Clear();
        }


        #endregion



        #region Private methods

        private void Level_OnLevelStateChanged(LevelState state)
        {
            switch (state)
            {
                case LevelState.AllTargetsHitted:
                    PlayCompleteAnimation();
                    break;
                case LevelState.StageChanging:
                    break;
            }
        }

        private void Level_OnLevelEnd(LevelResult obj)
        {
            PlayCompleteAnimation();
        }


        private void PlayCompleteAnimation()
        {
            IngameScreen screen = UiScreenManager.Instance.LoadedScreen<IngameScreen>(ScreenType.Ingame);
            if (!screen.IsNull())
            {
                if (IngameScreen.IsUiEnabled)
                {
                    int index = levelEnvironment.Context.SublevelIndex;
                    if (index >= 0 && index < stages.Count)
                    {
                        StageUi stage = stages[index];
                        
                        if (!stage.IsNull())
                        {
                            stage.PlayCompleteAnimation();
                        }
                    }
                }
            }

        }

        #endregion
    }
}

