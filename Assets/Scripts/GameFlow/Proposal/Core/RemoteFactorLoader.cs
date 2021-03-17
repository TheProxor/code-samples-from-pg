using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Proposal
{
    public class RemoteFactorLoader : IAnyLoader<float>
    {
        #region Fields

        private readonly IAbTestService abTestService;

        #endregion



        #region Ctor

        public RemoteFactorLoader(IAbTestService _abTestService)
        {
            abTestService = _abTestService;
        }

        #endregion




        #region IAnyLoader

        public bool IsLoaded { get; private set; }

        public float LoadedObject { get; private set; }

        public void Load()
        {
            LoadedObject = abTestService.CommonData.reachSkinFactor;

            IsLoaded = true;
        }

        #endregion
    }
}