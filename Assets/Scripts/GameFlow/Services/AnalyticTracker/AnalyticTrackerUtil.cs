using Drawmasters.Levels;
using Modules.Analytics;

namespace Drawmasters.ServiceUtil
{
    public static class AnalyticTrackerUtil
    {
        public static CommonEvents.LevelResult ConvertForAnalytic(this LevelResult levelResult)
        {
            CommonEvents.LevelResult result = CommonEvents.LevelResult.Complete;

            switch (levelResult)
            {
                case LevelResult.Complete:
                    result = CommonEvents.LevelResult.Complete;
                    break;

                case LevelResult.Lose:
                    result = CommonEvents.LevelResult.Fail;
                    break;

                case LevelResult.Leave:
                    result = CommonEvents.LevelResult.Exit;
                    break;

                case LevelResult.Reload:
                    result = CommonEvents.LevelResult.Reload;
                    break;

                case LevelResult.IngameSkip:
                case LevelResult.ResultSkip:
                    result = CommonEvents.LevelResult.Skip;
                    break;
                default:
                    //TODO
                    // CustomDebug.Log("Wrong analytic field.");
                    break;
            }

            return result;
        }
    }
}

