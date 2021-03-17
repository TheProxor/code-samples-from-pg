using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Modules.General;


namespace Drawmasters.Ui.Behaviours
{
    public class SingleRewardBehaviour : RewardBehaviour
    {
        #region Ctor

        public SingleRewardBehaviour(MultipleRewardScreen.Data _commonData,
            List<UiRewardLayout.Data> _layoutData,
            MultipleRewardScreen _screen,
            Action<RewardData[]> callback): base(_commonData, _layoutData, _screen, callback)
        {
        }

        #endregion



        #region IUiBehaviour

        public override void Enable()
        {
            base.Enable();

            layoutHelper.PermormRewardsLayout(screen.SortingOrder);
            Scheduler.Instance.CallMethodWithDelay(this, 
                () =>
                {
                    CommonUtility.SetObjectActive(commonData.tapInfoGameObject, true);
                    LeagueProposeController controller = GameServices.Instance.ProposalService.LeagueProposeController;
                    controller.VisualSettings.mainRewardFadeAnimation.Play(value =>
                    { 
                        commonData.canvasGroupTapToContinue.alpha = value;
                    }, this);
                    commonData.skipButton.onClick.AddListener(SkipButton_OnClick);
                }, 
                IngameData.Settings.league.leagueRewardSettings.durationClaimReward);
        }

        
        public override void SetRewards(RewardData[] _rewards)
        {
            List<UiRewardLayout.Data> tmpLayoutData;
            
            rewards = _rewards.Where(x => x.IsSkinReward()).ToList();
            
            if (rewards.Count > 0)
            {
                if (rewards.Count > 1)
                {
                    remainingRewards = rewards.GetRange(1, rewards.Count - 1).ToList();
                    rewards.RemoveRange(1, rewards.Count - 1);
                }
                
                tmpLayoutData = layoutData.Where(x => x.isShowSkin).ToList();
            }
            else
            {
                tmpLayoutData = layoutData.Where(x => !x.isShowSkin).ToList();
            }

            rewards.AddRange(_rewards.Where(x => !x.IsSkinReward()));
            
            layoutHelper = new UiRewardLayout(tmpLayoutData, rewards.ToArray());
        }

        #endregion
    }
}
