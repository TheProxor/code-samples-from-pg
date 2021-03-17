using Drawmasters.ServiceUtil;
using Drawmasters.Effects;
using UnityEngine;
using ObjectState = Drawmasters.Levels.BonusLevelBehaviourComponent.BonusLevelObjectState;


namespace Drawmasters.Levels
{
    public class PhysicalObjectBonusLevelShineComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        private bool isComponentEnabled;
        private BonusLevelSettings settings;
        private EffectHandler shineEffectHandler;

        #endregion



        #region Properties

        private bool CanUseShine
        {
            get
            {
                Proposal.RewardType rewardType = sourceLevelObject.CurrentData.bonusData.rewardType;

                if (rewardType == Proposal.RewardType.None)
                {
                    CustomDebug.Log("Reward type can't be <b>None</b>");
                    return false;
                }

                return rewardType != Proposal.RewardType.Currency;
            }
        }

        #endregion



        #region Abstract implementation

        public override void Initialize(CollisionNotifier notifier, Rigidbody2D rigidbody, PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);

            isComponentEnabled = GameServices.Instance.LevelEnvironment.Context.IsBonusLevel &&
                                 sourceObject.PhysicalData.type == PhysicalLevelObjectType.Bonus && 
                                 CanUseShine;

            settings = IngameData.Settings.bonusLevelSettings;
        }


        public override void Enable()
        {
            if (!isComponentEnabled)
            {
                return;
            }

            BonusLevelBehaviourComponent.OnStateChanged += BonusLevelBehaviourComponent_OnStateChanged;
        }


        public override void Disable()
        {
            if (!isComponentEnabled)
            {
                return;
            }

            BonusLevelBehaviourComponent.OnStateChanged -= BonusLevelBehaviourComponent_OnStateChanged;
        }

        #endregion



        #region Methods

        private void StartShine()
        {
            shineEffectHandler = EffectManager.Instance.CreateSystem(EffectKeys.FxGUIBonusLevelPetShine, true, sourceLevelObject.CenterPosition, default, sourceLevelObject.transform);

            if(shineEffectHandler != null)
            {
                shineEffectHandler.Play();
            }
        }

        private void FinishShine()
        {
            if (shineEffectHandler == null)
            {
                return;
            }

            EffectManager.Instance.ReturnHandlerToPool(shineEffectHandler);
            shineEffectHandler = null;
        }

        #endregion



        #region Events handlers

        private void BonusLevelBehaviourComponent_OnStateChanged(PhysicalLevelObject anotherObject, ObjectState state)
        {
            if (sourceLevelObject == anotherObject)
            {
                if (state == ObjectState.Stopped)
                {
                    StartShine();
                }
                else if (state == ObjectState.Unstopped || state == ObjectState.Thrown)
                {
                    FinishShine();
                }
            }
        }

        #endregion
    }
}
