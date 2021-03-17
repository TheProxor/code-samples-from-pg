using System;
using Drawmasters.Proposal;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class ForcemeterRewardComponent : ForcemeterComponent
    {
        #region Fields

        public static event Action<ForcemeterRewardElement[]> OnRewardApplied;

        private readonly ForcemeterRewardElement[] elements;

        #endregion
        


        #region Class lifecycle

        public ForcemeterRewardComponent(ref ForcemeterRewardElement[] _elements)
        {
            elements = _elements;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            ForceMeterScreen.OnRewardSetted += ForceMeterScreen_OnRewardSetted;
        }


        public override void Disable()
        {
            ForceMeterScreen.OnRewardSetted -= ForceMeterScreen_OnRewardSetted;

            if (elements != null)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i] != null)
                    {
                        elements[i].Deinitialize();
                    }
                }
            }
        }

        #endregion



        #region Events handlers

        private void ForceMeterScreen_OnRewardSetted(RewardData[] data)
        {
            if (elements.Length != data.Length)
            {
                CustomDebug.Log("Wrong data length");
            }

            for (int i = 0; i < elements.Length && i < data.Length; i++)
            {
                elements[i].Initialize(data[i]);
            }

            OnRewardApplied?.Invoke(elements);
        }

        #endregion
    }
}
