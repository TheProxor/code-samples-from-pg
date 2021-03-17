using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Proposal;
using Drawmasters.Ui.Behaviours;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class MultipleRewardScreen : AnimatorScreen
    {
        #region Helpers
        
        [Serializable]
        public class Data
        {
            [Required] public Button skipButton = default;
            [Required] public GameObject tapInfoGameObject = default;
            [Required] public Localize rewardTextLocalize = default;
            [Required] public CanvasGroup canvasGroupTapToContinue = default;
        }
        
        #endregion



        #region Fields

        [SerializeField] private Data commonData = default;
        [SerializeField] private ChestRewardBehaviour.Data chestData = default;
        
        [FormerlySerializedAs("chestLayoutData")] 
        [SerializeField] private List<UiRewardLayout.Data> rewardItemsLayoutData = default;
        
        private readonly Dictionary<MultipleRewardScreenBehaviourType, RewardBehaviour> behaviours =
            new Dictionary<MultipleRewardScreenBehaviourType, RewardBehaviour>();

        #endregion
        
        
        
        #region Abstract implementation
        
        public override ScreenType ScreenType => ScreenType.MultipleReward;


        public override void Deinitialize()
        {
            foreach (var i in behaviours.Values)
            {
                i.Deinitialize();
            }

            behaviours.Clear();

            base.Deinitialize();
        }

        public override void DeinitializeButtons(){ }

        public override void InitializeButtons(){ }
        
        #endregion
        
        
        
        #region Public methods

        public void SetupReward(RewardData[] rewards)
        {
            RewardDataUtility.OpenRewards(rewards);

            behaviours.Add(MultipleRewardScreenBehaviourType.ChestBehaviour, 
                new ChestRewardBehaviour(commonData, 
                                         chestData, 
                                         rewardItemsLayoutData, 
                                         this,
                                         ShowNextRewards));
            
            behaviours.Add(MultipleRewardScreenBehaviourType.SingleRewardBehaviour,
                new SingleRewardBehaviour(commonData,
                                          rewardItemsLayoutData,
                                          this,
                                          ShowNextRewards));

            ShowNextRewards(rewards);
        }


        private void ShowNextRewards(RewardData[] rewards)
        {
            if (rewards == null || rewards.Length == 0)
            {
                Hide();
                return;
            }
            
            MultipleRewardScreenBehaviourType behaviourType = rewards.Any(x => x.Type == RewardType.Chest) ?
                MultipleRewardScreenBehaviourType.ChestBehaviour : MultipleRewardScreenBehaviourType.SingleRewardBehaviour;

            RewardBehaviour currentBehaviour = behaviours[behaviourType];

            foreach (var behaviour in behaviours.Values)
            {
                behaviour.Disable();

                if (behaviour == currentBehaviour)
                {
                    behaviour.SetRewards(rewards);
                    behaviour.Enable();
                }
            }
        }


        public void SetRewardTextKey(string key)
        {
            CommonUtility.SetObjectActive(commonData.rewardTextLocalize.gameObject, true);

            if (string.IsNullOrEmpty(key))
            {
                CommonUtility.SetObjectActive(commonData.rewardTextLocalize.gameObject, false);
            }

            commonData.rewardTextLocalize.SetTerm(key);
        }
        
        #endregion
    }
}
