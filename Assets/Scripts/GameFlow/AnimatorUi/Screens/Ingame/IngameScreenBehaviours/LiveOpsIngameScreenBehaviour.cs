using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drawmasters.Ui
{
    public class LiveOpsIngameScreenBehaviour : BaseIngameScreenBehaviour
    {
        #region Helper types

        [Serializable]
        public class LiveOpsData
        {
            public GameObject bulletsCounterRootGameObject = default;
            public List<BulletUi> bullets = default;

            public VectorAnimation scaleInAnimation = default;
            public VectorAnimation scaleOutAnimation = default;

            public IngameBulletsUi.Data[] spritesData = default;
        }

        #endregion



        #region Fields

        private IngameBulletsUi.Counter bulletsCounter;

        private readonly LiveOpsData liveOpsData;

        #endregion



        #region Abstract implementation

        protected override Announcer.Announcer[] AvailableAnnouncers 
            => Array.Empty<Announcer.Announcer>();

        protected override bool IsSkipButtonAvailable => true;

        public override bool IsCallPetButtonAvailable => true;

        #endregion



        #region Ctor

        public LiveOpsIngameScreenBehaviour(IngameScreen ingameScreen, Data _data, LiveOpsData _liveOpsData) 
            : base(ingameScreen, _data)
        {
            liveOpsData = _liveOpsData;
        }

        #endregion



        #region Overrided methods

        public override void Initialize()
        {
            base.Initialize();

            bulletsCounter = new IngameBulletsUi.Counter(liveOpsData.bullets);
            bulletsCounter.InitializeAnimation(liveOpsData.scaleInAnimation, liveOpsData.scaleOutAnimation);            
            bulletsCounter.Initialize();

            WeaponType weaponType = GameServices.Instance.LevelEnvironment.Context.WeaponType;

            IngameBulletsUi.Data weaponData = liveOpsData.spritesData.Find(i => i.weaponType == weaponType);
            if (weaponData == null)
            {
                CustomDebug.Log($"Cannot find weapon data. For weapon type: {weaponType}");
            }

            bulletsCounter.InitializeVisual(weaponData);

            CommonUtility.SetObjectActive(liveOpsData.bulletsCounterRootGameObject, true);
        }


        public override void Deinitialize()
        {
            bulletsCounter.Deinitialize();

            base.Deinitialize();
        }

        #endregion
    }
}

