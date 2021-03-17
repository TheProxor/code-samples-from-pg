using System;
using Drawmasters.Announcer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Drawmasters.Ui
{
    public class CommonBehaviour : BaseIngameScreenBehaviour
    {
        #region Fields

        [Serializable]
        public class CommonData
        {
            [Required] public Transform shooterKillAnimatable = default;
            [Required] public Transform hostageKillAnimatable = default;
            [Required] public Transform hostageLevelAnimatable = default;
        }

        #endregion
        
        
        
        #region Fields

        private readonly CommonData commonData;
        
        #endregion
        
        
        
        public CommonBehaviour(IngameScreen ingameScreen, 
                               Data _data,
                               CommonData _commonData)
            : base(ingameScreen, _data)
        {
            commonData = _commonData;
        }

        protected override Announcer.Announcer[] AvailableAnnouncers
            => new Announcer.Announcer[]
            {
                new ShooterKillAnnouncer(commonData.shooterKillAnimatable), 
                new HostageKillAnnouncer(commonData.hostageKillAnimatable), 
                new OutOfInkAnnouncer(data.outOfInkAnimatable),
                new HostageLevelAnnouncer(screen, commonData.hostageLevelAnimatable)
            };
        
        protected override bool IsSkipButtonAvailable => true;

        public override bool IsCallPetButtonAvailable => true;
    }
}

