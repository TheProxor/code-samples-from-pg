using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Levels
{
    public static class LevelFinisherExtension
    {
        public static ILevelFinisher DefineFinisher(this ILevelFinisher thisFinisher)
        {
            ILevelFinisher finisher;

            ILevelEnvironment levelEnvironment = GameServices.Instance.LevelEnvironment;

            LevelContext context = levelEnvironment.Context;
            LevelProgress progress = levelEnvironment.Progress;

            switch (progress.LevelResultState)
            {
                case LevelResult.IngameSkip:
                case LevelResult.ResultSkip:
                case LevelResult.Complete:
                    if (!context.IsEndOfLevel)
                    {
                        if (context.Mode.IsHitmastersLiveOps())
                        {
                            finisher = new HitmastersAnimatedLevelFinisher();
                        }
                        else
                        {
                            finisher = new CommonAnimatedLevelFinisher();
                        }
                    }
                    else
                    {
                        if (context.Mode.IsHitmastersLiveOps())
                        {
                            finisher = new HitmastersCompleteLevelFinisher();
                        }
                        else
                        {
                            finisher = new CompleteLevelFinisher(GameServices.Instance.ProposalsLoader);
                        }
                    }
                    break;

                case LevelResult.Lose:
                    finisher = new LoseLevelFinisher();
                    break;

                case LevelResult.Leave:
                    if (context.Mode.IsHitmastersLiveOps())
                    {
                        finisher = new HitmastersLeaveLevelFinisher();
                    }
                    else
                    {
                        finisher = new LeaveLevelFinisher();
                    }
                    break;

                case LevelResult.Reload:
                    finisher = new ReloadLevelFinisher();
                    break;

                case LevelResult.ProposalEnd:
                    if (context.Mode.IsHitmastersLiveOps())
                    {
                        finisher = new HitmastersProposalCompleteLevelFinisher();
                    }
                    else
                    {
                        finisher = new AfterProposalLevelFinisher();
                    }
                    break;

                default:
                    finisher = new LeaveLevelFinisher();
                    break;
            }

            return finisher;
        }
    }
}
