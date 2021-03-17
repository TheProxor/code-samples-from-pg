using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public static class LevelHeaderLoaderCreator
    {
        public static IHeaderLoader CreateLoader(GameMode gameMode,
                                                 GameMode specialScene,
                                                 bool isEditor)
        {
            bool isSpecialScene = specialScene != GameMode.None;

            if (isEditor)
            {
                return new EditorHeaderLoader();
            }

            if (isSpecialScene)
            {
                return new SceneHeaderLoader();
            }

            if (specialScene == GameMode.ForcemeterScene)
            {
                return new ForcemeterHeaderLoader();
            }

            bool isBossLevelsEnabled = GameServices.Instance.AbTestService.CommonData.isBossLevelsEnabled;            
            if (isBossLevelsEnabled || gameMode.IsHitmastersLiveOps())
            {
                return new CommonHeaderLoader();
            }
            else
            {
                return new AbTestHeaderLoader();
            }            
        }
    }
}
