using System;
using System.Collections.Generic;
using DG.Tweening;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Modules.General;


namespace Drawmasters.Ui.Behaviours
{
    public abstract class RewardBehaviour : IUiBehaviour
    {
        #region Fields

        protected readonly MultipleRewardScreen.Data commonData;
        protected readonly List<UiRewardLayout.Data> layoutData;
        protected readonly MultipleRewardScreen screen;
        protected readonly Action<RewardData[]> onApplyRewards;

        protected UiRewardLayout layoutHelper;
        protected List<RewardData> rewards;
        protected List<RewardData> remainingRewards;

        #endregion
        
        
        
        #region Ctor

        public RewardBehaviour(MultipleRewardScreen.Data _commonData,
                                    List<UiRewardLayout.Data> _layoutData,
                                    MultipleRewardScreen _screen, 
                                    Action<RewardData[]> callback)
        {
            commonData = _commonData;
            layoutData = _layoutData;
            screen = _screen;
            onApplyRewards = callback;

            remainingRewards = new List<RewardData>();
        }
        
        #endregion
        
        
        
        #region IUiBehaviour

        public virtual void Enable()
        {
            InitializeButtons();
            CommonUtility.SetObjectActive(commonData.tapInfoGameObject, false);
        }


        public virtual void Disable()
        {
            DeinitializeButtons();
            CommonUtility.SetObjectActive(commonData.tapInfoGameObject, false);
            layoutHelper?.Clear();
            remainingRewards?.Clear();
            rewards?.Clear();
        }


        public virtual void InitializeButtons() { }


        public virtual void DeinitializeButtons() { }


        public virtual void Deinitialize()
        {
            DeinitializeButtons();

            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            layoutHelper?.Clear();
        }


        public abstract void SetRewards(RewardData[] _rewards);
        
        #endregion

        
        
        #region Events handlers
        
        protected  void SkipButton_OnClick()
        {
            commonData.skipButton.onClick.RemoveListener(SkipButton_OnClick);

            layoutHelper?.DeinitializeItems();
            
            LeagueProposeController controller = GameServices.Instance.ProposalService.LeagueProposeController;
            
            Scheduler.Instance.CallMethodWithDelay(this, Claim, controller.VisualSettings.hideRewardTimeout);
        }

        protected void Claim()
        {
            RewardDataUtility.ApplyRewards(rewards.ToArray());

            onApplyRewards?.Invoke(remainingRewards.ToArray());
        }

        #endregion
    }
}