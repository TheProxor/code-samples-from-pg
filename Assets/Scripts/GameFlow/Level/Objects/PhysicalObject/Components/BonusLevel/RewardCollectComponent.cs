using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;
using Drawmasters.Proposal;
using System;
using UnityEngine;
using Modules.General;
using ObjectState = Drawmasters.Levels.BonusLevelBehaviourComponent.BonusLevelObjectState;


namespace Drawmasters.Levels
{
    public class RewardCollectComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        public static event Action<PhysicalLevelObject, BonusLevelObjectData> OnRewardDropped;

        private BonusLevelController bonusLevelController;

        private BonusLevelObjectData bonusData;

        private bool isRewardDropped;
        private bool wasObjectThrown;

        private const RewardType defaultRewardType = RewardType.Currency;

        #endregion
        
        
        
        #region Properties
        
        public bool IsComponentEnabled { get; private set; }

        #endregion



        #region Overrided methods

        public override void Initialize(CollisionNotifier notifier, 
                                        Rigidbody2D rigidbody, 
                                        PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);

            bonusData = sourceObject.CurrentData.bonusData;

            IsComponentEnabled = CanCollectReward();

            bonusLevelController = GameServices.Instance.LevelControllerService.BonusLevelController;

            bool CanCollectReward()
            {
                bool result = false;

                switch(bonusData.rewardType)
                {
                    case RewardType.None:
                        bonusData.rewardType = defaultRewardType;
                        result = CanCollectReward();
                        break;

                    case RewardType.Currency:
                        result = bonusData.currencyType != CurrencyType.None &&
                                 bonusData.currencyAmount > 0f;
                        break;

                    case RewardType.PetSkin:
                        result = bonusData.petSkinType != PetSkinType.None;
                        break;

                    default:
                        CustomDebug.Log($"Reward Type <b>{bonusData.rewardType}</b> is not supported for collect");
                        result = false;
                        break;
                }

                return result;
            }
        }

        public override void Enable()
        {
            if (IsComponentEnabled)
            {
                collisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;

                sourceLevelObject.MainCollider2D.isTrigger = true;
                isRewardDropped = false;

                BonusLevelBehaviourComponent.OnStateChanged += BonusLevelBehaviourComponent_OnStateChanged;
            }
        }


        public override void Disable()
        {
            if (IsComponentEnabled)
            {
                collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;

                sourceLevelObject.MainCollider2D.isTrigger = false;

                wasObjectThrown = false;

                BonusLevelBehaviourComponent.OnStateChanged -= BonusLevelBehaviourComponent_OnStateChanged;

                Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            }
        }

        #endregion



        #region Methods

        private void CollectReward()
        {
            isRewardDropped = true;

            if (bonusData.rewardType == RewardType.Currency)
            {
                CollectCurrencyReward();
            }
         
            OnRewardDropped?.Invoke(sourceLevelObject, bonusData);


            void CollectCurrencyReward()
            {
                CurrencyType currencyType = bonusData.currencyType;
                float amount = bonusData.currencyAmount;

                if (currencyType.IsMansionCurrency() && !currencyType.IsMansionAvailableForShow())
                {
                    currencyType = CurrencyType.Simple;
                    amount *= 10.0f;
                }
                else if (currencyType.IsMonopolyCurrency() && !currencyType.IsMonopolyAvailableForShowOnLevel())
                {
                    currencyType = CurrencyType.Premium;
                }

                GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(currencyType, amount);
            }
        }


        private void CollectMandatoryReward()
        {
            if (bonusData.rewardType == RewardType.Currency)
            {
                return;
            }

            CollectReward();
            sourceLevelObject.DestroyObject();
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D otherCollider)
        {
            if (isRewardDropped)
            {
                return;
            }

            CollidableObject collidableObject = otherCollider.gameObject.GetComponent<CollidableObject>();

            if (collidableObject.Projectile != null)
            {
                CollectReward();
            }  
        }


        private void BonusLevelBehaviourComponent_OnStateChanged(PhysicalLevelObject anotherObject, ObjectState objectState)
        {
            if (anotherObject != sourceLevelObject)
            {
                return;
            }

            wasObjectThrown = !wasObjectThrown ? objectState == ObjectState.Thrown : wasObjectThrown;

            if (wasObjectThrown)
            {
                if (objectState == ObjectState.Deceleration)
                {
                    Scheduler.Instance.CallMethodWithDelay(this, CollectMandatoryReward, IngameData.Settings.bonusLevelSettings.decelerationDelay +
                        IngameData.Settings.bonusLevelSettings.mandatoryRewardAutoCollectDelay);
                }
                else if (objectState == ObjectState.Stopped)
                {
                    Scheduler.Instance.UnscheduleMethod(this, CollectMandatoryReward);
                }
                else if (objectState == ObjectState.Unstopped)
                {
                    CollectMandatoryReward();
                }
            }
        }
           
        #endregion
    }
}

