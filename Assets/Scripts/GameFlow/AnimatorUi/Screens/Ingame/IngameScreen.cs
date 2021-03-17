using System;
using Drawmasters.Levels.Data;
using UnityEngine;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using UnityEngine.UI;

namespace Drawmasters.Ui
{
    public partial class IngameScreen : RewardReceiveScreen
    {
        #region Fields

        [Header("Base data")]
        [SerializeField] private BaseIngameScreenBehaviour.Data baseData = default;

        [Header("Common data")]
        [SerializeField] private CommonBehaviour.CommonData commonData = default;

        [Header("Boss data")]
        [SerializeField] private BossBehaviour.BossData bossData = default;

        [Header("Bonus data")]
        [SerializeField] private BonusBehaviour.BonusData bonusData = default;

        [Header("LiveOps data")]
        [SerializeField] private LiveOpsIngameScreenBehaviour.LiveOpsData liveOpsData = default;

        private BaseIngameScreenBehaviour behaviour;

        #endregion



        #region Properties

        public override ScreenType ScreenType => ScreenType.Ingame;

        protected override string IdleAfterShowKey => AnimationKeys.IngameScreen.IdleAfterShow;

        public Vector3 SlowmoVfxPosition => bonusData.bonusLevelAnimatable.position;

        public Vector3 PetStartPosition => baseData.petsData.callPetButtonActive.transform.position;

        public GameObject CallPetButtonMainRoot => baseData.petsData.callPetButtonMainRoot;

        public bool IsCallPetButtonAvailable => behaviour.IsCallPetButtonAvailable;

        #endregion



        #region Overrided methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBegin = null,
                                        Action<AnimatorView> onHideBegin = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBegin, onHideBegin);

            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            if (context.IsBonusLevel)
            {
                behaviour = new BonusBehaviour(this, baseData, bonusData, uiHudTop);
            }          
            else if (context.IsBossLevel)
            {
                if (context.Mode.IsHitmastersLiveOps())
                {
                    behaviour = new LiveOpsBossIngameScreenBehaviour(this, baseData, liveOpsData, bossData);
                }
                else
                {
                    behaviour = new BossBehaviour(this, baseData, bossData);
                }
            }
            else if (context.Mode.IsHitmastersLiveOps())
            {
                behaviour = new LiveOpsIngameScreenBehaviour(this, baseData, liveOpsData);
            }
            else
            {
                behaviour = new CommonBehaviour(this, baseData, commonData);
            }
            
            behaviour.Initialize();

            //TODO dirty; 
            if(!IsUiEnabled)
            {
                IsUiEnabled = isUiEnabled;
            }
        }


        public override void Deinitialize()
        {
            behaviour?.Deinitialize();

            base.Deinitialize();
        }

        public override void DeinitializeButtons()
        {
            behaviour?.DeinitializeButtons();
        }

        public override void InitializeButtons()
        {
            behaviour?.InitializeButtons();
        }

        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        => Vector3.zero; // DUMMY

        #endregion
    }
}
