using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Settings;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using Modules.General;
using Modules.Sound;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Ui.Behaviours
{
    public class ChestRewardBehaviour : RewardBehaviour
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            [Required] public SkeletonGraphic chestAnimation = default;
            [Required] public AnimationEffectPlayer animationEffectPlayer = default;
            [Required] public Canvas chestCanvas = default;
        }

        #endregion
        
        
        
        #region Fields
        
        private Data data;
        private ChestAnimationSettings chestAnimationSettigs;
        private ChestReward chest;

        #endregion
        
        
        
        #region Ctor

        public ChestRewardBehaviour(MultipleRewardScreen.Data _commonData,
                                    Data data,
                                    List<UiRewardLayout.Data> _layoutData,
                                    MultipleRewardScreen _screen, 
                                    Action<RewardData[]> callback) : base(_commonData, _layoutData, _screen, callback)
        {
            this.data = data;
            
            chestAnimationSettigs = IngameData.Settings.league.leagueRewardSettings.chestAnimationSettins;
        }
        
        #endregion
        
        
        
        #region IUiBehaviour
        
        public override void Enable()
        {
            base.Enable();
            ApplyChestVisual();
            
            CommonUtility.SetObjectActive(data.chestAnimation.gameObject, true);
            CommonUtility.SetObjectActive(commonData.tapInfoGameObject, false);
            Scheduler.Instance.CallMethodWithDelay(this, OpenChest, chestAnimationSettigs.durationOpen);
        }


        public override void Disable()
        {
            CommonUtility.SetObjectActive(data.chestAnimation.gameObject, false);

            data.animationEffectPlayer.Deinitialize();
            data.animationEffectPlayer.OnEventHappend -= AnimationEffectPlayer_OnEventHappened;
            
            base.Disable();
        }
        

        public override void Deinitialize()
        {
            data.animationEffectPlayer.Deinitialize();
            data.animationEffectPlayer.OnEventHappend -= AnimationEffectPlayer_OnEventHappened;

            base.Deinitialize();
        }


        public override void SetRewards(RewardData[] _rewards)
        {
            List<UiRewardLayout.Data> tmpLayoutData;

            remainingRewards = _rewards.Where(x => x.Type != RewardType.Chest).ToList();
            chest = (ChestReward)_rewards.Find(x => x.Type == RewardType.Chest);
            
            if (chest != null)
            {
                rewards = chest.ContainedRewards.ToList();
                
                if (rewards.Any(x => x.IsSkinReward()))
                {
                    tmpLayoutData = layoutData.Where(x => x.isShowSkin).ToList();
                }
                else
                {
                    tmpLayoutData = layoutData.Where(x => !x.isShowSkin).ToList();
                }
                layoutHelper = new UiRewardLayout(tmpLayoutData, rewards.ToArray());
            }
        }
        
        #endregion



        #region Private methods

        private void ApplyChestVisual()
        {
            string skinName = IngameData.Settings.league.leagueRewardSettings.FindChestSkin(chest.chestType);
            data.chestAnimation.Skeleton.SetSkin(skinName);

            data.chestCanvas.sortingOrder = screen.SortingOrder + 4;
            data.chestCanvas.sortingLayerID = LayerMask.NameToLayer(RenderLayers.Ui);
            data.chestCanvas.sortingLayerName = RenderLayers.Ui;

            data.chestAnimation.AnimationState.SetAnimation(0, chestAnimationSettigs.appearAnimationName, false);
            data.chestAnimation.AnimationState.AddAnimation(0, chestAnimationSettigs.idleAnimationName, true, 0f);
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.CHEST_SKULL_APPEAR);
        }

        #endregion
        
        
        
        #region Events handlers
        
        private void OpenEntry_OnComplete(TrackEntry trackEntry)
        {
            layoutHelper.PermormRewardsLayout(screen.SortingOrder);

            data.chestAnimation.AnimationState.SetAnimation(0, chestAnimationSettigs.outAnimationName, false);
            data.animationEffectPlayer.Deinitialize();
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.CHEST_SKULL_LOOT_APPEAR);
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
                chestAnimationSettigs.durationOpen);
        }


        private void OpenChest()
        {
            TrackEntry openEntry = data.chestAnimation.AnimationState.SetAnimation(0, chestAnimationSettigs.openAnimationName, false);

            data.animationEffectPlayer.Initialize();
            data.animationEffectPlayer.OnEventHappend += AnimationEffectPlayer_OnEventHappened;

            openEntry.Complete += OpenEntry_OnComplete;
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.CHEST_SKULL_OPEN);
        }


        private void AnimationEffectPlayer_OnEventHappened(AnimationEffectPlayer e)
        {
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.CHESTOPEN);
            e.SetSortingOrders(screen.SortingOrder + 4);
        }

        #endregion
    }
}