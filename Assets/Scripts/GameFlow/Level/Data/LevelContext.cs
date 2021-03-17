using Drawmasters.Levels.Order;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Levels.Data
{
    public class LevelContext
    {
        #region Properties

        public GameMode Mode { get; private set; }


        public int Index { get; private set; }


        public bool IsEditor { get; private set; }


        public int ColorProfileIndex { get; private set; }


        public WeaponType WeaponType { get; private set; }


        public int ProjectilesCount { get; private set; }


        public GameMode SceneMode { get; private set; }


        public bool IsProposalSceneFromRewardData { get; private set; }


        public bool IsFirstSublevel { get; private set; }


        public bool IsEndOfLevel { get; private set; }


        public bool IsEndOfChapter { get; private set; }


        public int SublevelsCount { get; private set; }


        public int SublevelIndex { get; private set; }


        public int ChapterIndex { get; private set; }


        public bool IsBonusLevel { get; private set; }


        public bool IsHostagesLevel { get; private set; }


        public bool IsSceneMode { get; private set; }


        public string LevelId { get; private set; }


        public bool IsBossLevel { get; private set; }


        public float PathDistance { get; private set; }


        public int LevelGlobalIndex { get; private set; }


        public int LevelLocalIndex { get; private set; }


        public LevelType LevelType { get; private set; }

        #endregion



        #region Methods

        public void LoadContext(GameMode mode,
                                int index,
                                bool isHostageLevel,
                                bool isEditor,
                                WeaponType weaponType,
                                GameMode sceneMode,
                                LevelType levelType,
                                float pathDistance,
                                int projectilesCount,
                                bool isPremiumProposalScene)
        {
            Mode = mode;
            Index = index;
            IsEditor = isEditor;
            IsHostagesLevel = isHostageLevel;
            
            PathDistance = pathDistance;
            ProjectilesCount = projectilesCount;
            IsProposalSceneFromRewardData = isPremiumProposalScene;

            ILevelOrderService levelOrderService = LevelsOrderServiceSelector.Select(Mode);
            ILevelGraphicService levelGraphicService = GameServices.Instance.LevelGraphicService;

            SublevelData? data = levelOrderService.FindData(Mode, Index);
            if (data == null)
            {
                return;
            }

            if (IsEditor)
            {
                ColorProfileIndex = 0;
            }
            else if (IsProposalSceneFromRewardData)
            {
                ColorProfileIndex = levelGraphicService.RandomRewardProposalSceneIndex;
            }
            else
            {
                ColorProfileIndex = levelGraphicService.GetLevelProfileIndex(Mode, data.Value.ChapterIndex, Index);
            }

            LevelGlobalIndex = data.Value.LevelGlobalIndex;
            LevelLocalIndex = data.Value.LevelLocalIndex;

            IsBonusLevel = levelType == LevelType.Bonus;
            IsBossLevel = levelType == LevelType.Boss;

            WeaponType = weaponType;
            
            SceneMode = sceneMode;
            IsSceneMode = SceneMode.IsSceneMode();

            if (Mode.IsExcludedFromLoad())
            {
                return;
            }

            IsFirstSublevel = data.Value.SublevelLocalIndex == 0;
            IsEndOfLevel = data.Value.IsEndOfLevel;
            IsEndOfChapter = data.Value.IsEndOfChapter;

            SublevelsCount = data.Value.SublevelsCount;
            SublevelIndex = data.Value.SublevelLocalIndex;

            ChapterIndex = data.Value.ChapterIndex;

            LevelType = levelType;

            LevelId = data.Value.NameId;
        }

        #endregion
    }
}
