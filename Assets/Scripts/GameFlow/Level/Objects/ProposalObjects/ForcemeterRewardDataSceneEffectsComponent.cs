using Drawmasters.Effects;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class ForcemeterRewardDataSceneEffectsComponent : ForcemeterComponent
    {
        #region Fields

        private readonly IdleEffect[] idleEffects;

        #endregion



        #region Class lifecycle

        public ForcemeterRewardDataSceneEffectsComponent(IdleEffect[] _idleEffects)
        {
            idleEffects = _idleEffects;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            bool isForcemeterRewardDataScene = GameServices.Instance.LevelEnvironment.Context.IsProposalSceneFromRewardData &&
                                               GameServices.Instance.LevelEnvironment.Context.SceneMode == GameMode.ForcemeterScene;
            if (!isForcemeterRewardDataScene)
            {
                return;
            }

            foreach (var e in idleEffects)
            {
                e.CreateAndPlayEffect();
            }
        }


        public override void Disable()
        {
            foreach (var e in idleEffects)
            {
                e.StopEffect();
            }
        }

        #endregion
    }
}
