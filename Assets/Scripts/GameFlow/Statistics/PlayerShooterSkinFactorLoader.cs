using System.Collections.Generic;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Statistics
{
    public class PlayerShooterSkinFactorLoader
    {
        #region Fields

        private readonly List<IAnyLoader<float>> loaders;

        #endregion



        #region Properties

        public float LoadedFactor { get; private set; }

        #endregion



        #region Ctor

        public PlayerShooterSkinFactorLoader(IAbTestService abTestService)
        {
            loaders = new List<IAnyLoader<float>>()
            {
                new RemoteFactorLoader(abTestService)
            };
        }

        #endregion



        #region Methods

        public void Load()
        {
            foreach(var loader in loaders)
            {
                loader.Load();

                if (loader.IsLoaded)
                {
                    LoadedFactor = loader.LoadedObject;
                    break;
                }
            }
        }

        #endregion
    }
}

