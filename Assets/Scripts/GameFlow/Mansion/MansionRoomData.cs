using System;
using Drawmasters.Proposal;


namespace Drawmasters.Mansion
{
    [Serializable]
    public class MansionRoomData
    {
        #region Fields

        public MansionRoomObjectType[] objectTypes = default;
        public ShooterSkinReward shooterSkinReward = default;
        [Enum(typeof(AudioKeys.Music))] public string musicKey = default;
        
        #endregion
    }
}
