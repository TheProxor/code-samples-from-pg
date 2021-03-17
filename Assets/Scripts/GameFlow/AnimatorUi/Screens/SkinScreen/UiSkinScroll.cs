using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Drawmasters.Proposal.Interfaces;
using System;
using DG.Tweening;
using Drawmasters.Proposal;
using Modules.Analytics;
using Modules.General.Abstraction;
using Sirenix.OdinInspector;
using TMPro;
using Object = UnityEngine.Object;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSkinScrollData
    {
        public GameObject tabRoot = default;
        [Required] public Transform cardsRoot = default;
        [Required] public RectTransform content = default;
        [Required] public ScrollRect scrollRect = default;
        [Required] public GameObject cardPrefab = default;
        [Required] public GameObject rewardRoot = default;
        [Required] public RewardedVideoButton rewardButton = default;
        [Required] public Button disabledRewardButton = default;
        [Required] public TMP_Text timeLeftRewardText = default;
        [Required] public Animator rewardButtonAnimator = default;
    }

    
    public abstract class UiSkinScroll<T, V> : UISkinScrollBehaviour where T : ChoosableCard<V>
    {
        #region Fields

        protected UiSkinScrollData data;
        
        private readonly List<T> cards = new List<T>();
        
        private Coroutine contentRoutine;

        public event Action<RewardData> OnShouldReceiveReward;
        
        #endregion


        
        #region Properties
        
        public abstract IProposable Proposal { get; }
        public abstract string VideoPlacementKey { get; }
        public abstract UiPanelRewardController Controller { get; }
        public abstract string AnimatorTrigerKey { get; }

        #endregion


        
        #region Methods

        public virtual void Enable()
        {
            CommonUtility.SetObjectActive(data.rewardRoot, false);
            
            if (data.tabRoot != null)
            {
                CommonUtility.SetObjectActive(data.tabRoot, true);
            }

            data.rewardButton.Initialize(VideoPlacementKey);
            data.rewardButton.OnVideoShowEnded += OnReceiveReward;
            data.rewardButton.InitializeButtons();
            

            if (contentRoutine == null)
            {
                contentRoutine = MonoBehaviourLifecycle.PlayCoroutine(SetupContent());
            }
            
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        public virtual void Disable()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            data.rewardButton.Deinitialize();
            data.rewardButton.OnVideoShowEnded -= OnReceiveReward;
            data.rewardButton.DeinitializeButtons();

            MonoBehaviourLifecycle.StopPlayingCorotine(contentRoutine);
            contentRoutine = null;

            if (data.tabRoot != null)
            {
                CommonUtility.SetObjectActive(data.tabRoot, false);
            }
        }


        public void InitializeButtons() { }


        public void DeinitializeButtons() { }


        public void Deinitialize()
        {
            Disable();
            DOTween.KillAll(true);
        }


        public void Clear()
        {
            CommonUtility.SetObjectActive(data.rewardRoot, false);

            foreach (var card in cards)
            {
                card.Deinitialize();
                Object.Destroy(card.gameObject);
            }

            cards.Clear();
        }


        protected void RecreateCards()
        {
            Clear();

            List<T> createdCards = CreateCards();
            cards.AddRange(createdCards);

            foreach (var card in cards)
            {
                card.Initialize();
            }

            Refresh();
        }


        protected void Refresh()
        {
            foreach (var card in cards)
            {
                card.RefreshView();
            }

            RefreshProposalView();
        }


        protected abstract List<T> CreateCards();
        
        private void RefreshProposalView()
        {
            CommonUtility.SetObjectActive(data.rewardRoot, Proposal?.IsAvailable ?? false);
            CommonUtility.SetObjectActive(data.rewardButton.gameObject, Proposal?.CanPropose ?? false);
            CommonUtility.SetObjectActive(data.disabledRewardButton.gameObject, !(Proposal?.CanPropose ?? false));
            
            RefreshTimeLeft();
        }

        private void RefreshTimeLeft()
        {
            data.timeLeftRewardText.text = Controller.Settings.UiTimeLeft;
        }


        private IEnumerator SetupContent()
        {
            RecreateCards();

            foreach (var card in cards)
            {
                card.PlayDisabledAnimation();
            }

            if (data.rewardButtonAnimator.gameObject.activeSelf)
            {
                data.rewardButtonAnimator.SetTrigger(AnimationKeys.SkinCard.Disabled);
                data.rewardButtonAnimator.Update(default);
            }

            yield return new WaitForEndOfFrame();

            T activeCard = cards.Find(e => e.IsActive);

            Vector2 cardScrollPosition = data.scrollRect.transform.InverseTransformPoint(data.content.position) -
                                         data.scrollRect.transform.InverseTransformPoint(activeCard.transform.position);

            float scrollContentBorder = (data.scrollRect.content.rect.size.x - data.scrollRect.viewport.rect.size.x) * 0.5f;
            float cardScrollPositionX = scrollContentBorder < 0.0f ? 0.0f : Mathf.Clamp(cardScrollPosition.x, -scrollContentBorder, scrollContentBorder);

            data.content.anchoredPosition = data.content.anchoredPosition.SetX(cardScrollPositionX);

            foreach (var card in cards)
            {
                card.PlayShowAnimation();
            }

            if (data.rewardButtonAnimator.gameObject.activeSelf)
            {
                data.rewardButtonAnimator.ResetTrigger(AnimationKeys.SkinCard.Disabled);
                data.rewardButtonAnimator.SetTrigger(AnimationKeys.SkinCard.Show);
            }
        }

        #endregion
        
        
        
        #region Events handlers
        
        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime) =>
            RefreshTimeLeft();
        
        
        private void OnReceiveReward(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                Proposal?.Propose(null);
                
                RewardData rewardData = Controller.Settings.GetRandomReward(Controller.ShowsCount);
                rewardData.Open();
            
                CommonEvents.SendAdVideoReward(VideoPlacementKey);

                OnShouldReceiveReward?.Invoke(rewardData);
                
                Controller.Settings.MarkVideoWatched();
                OnRewardReceived();
            }
        }


        protected virtual void OnRewardReceived() =>
            RecreateCards();
        
        #endregion
    }
}