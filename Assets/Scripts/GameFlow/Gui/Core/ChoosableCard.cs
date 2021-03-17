using UnityEngine;
using System;
using UnityEngine.UI;
using Modules.General.InAppPurchase;
using Drawmasters.ServiceUtil;
using Drawmasters.OffersSystem;
using Drawmasters.Ui;


namespace Drawmasters
{
    [Serializable]
    public class ChoosableCardVisualStateData
    {
        public ChoosableCardState state = default;
        public GameObject mainRoot = default;
        public GameObject commonRoot = default;
        public GameObject forSubscriptionRoot = default;
    }


    public abstract class ChoosableCard<T> : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Image[] icons = default;

        [SerializeField] private Button chooseButton = default;
        [SerializeField] private Button purchaseSubscriptionButton = default;

        [Header("View")]
        [SerializeField] private ChoosableCardVisualStateData[] visualStatesData = default;

        [SerializeField] private Animator cardAnimator = default;
        
        protected T currentType;
        private ChoosableCardState currentState;

        #endregion



        #region Properties

        public abstract bool IsActive { get; }

        public bool IsForSubscrption { get; private set; }

        #endregion



        #region Methods

        public virtual void Initialize()
        {
            chooseButton.onClick.AddListener(ChooseButton_OnClick);
            purchaseSubscriptionButton.onClick.AddListener(PurchaseSubscriptionButton_OnClick);
        }


        public virtual void Deinitialize()
        {
            chooseButton.onClick.RemoveListener(ChooseButton_OnClick);
            purchaseSubscriptionButton.onClick.RemoveListener(PurchaseSubscriptionButton_OnClick);
        }


        public virtual void SetupType(T type)
        {
            currentType = type;

            InitialRefresh();
            RefreshIcons();
        }


        public virtual void MarkForSubscription() =>
            IsForSubscrption = true;
        

        public void RefreshView()
        {
            ChoosableCardState state;

            if (IsForSubscrption && !SubscriptionManager.Instance.IsSubscriptionActive)
            {
                state = ChoosableCardState.WaitingForSubscriptionPurchase;
            }
            else if (!IsBought(currentType))
            {
                state = ChoosableCardState.Unbought;
            }
            else
            {
                state = IsActive ? ChoosableCardState.Active : ChoosableCardState.Inactive;
            }

            OnChangeCardState(state);
        }


        public void PlayShowAnimation()
        {
            cardAnimator.ResetTrigger(AnimationKeys.SkinCard.Disabled);
            cardAnimator.SetTrigger(AnimationKeys.SkinCard.Show);
        }


        public void PlayDisabledAnimation()
        {
            cardAnimator.SetTrigger(AnimationKeys.SkinCard.Disabled);
            cardAnimator.Update(default);
        }

        protected virtual void OnChangeCardState(ChoosableCardState state)
        {
            currentState = state;

            foreach (var data in visualStatesData)
            {
                CommonUtility.SetObjectActive(data.mainRoot, currentState == data.state);

                if (currentState == data.state)
                {
                    CommonUtility.SetObjectActive(data.commonRoot, !IsForSubscrption);
                    CommonUtility.SetObjectActive(data.forSubscriptionRoot, IsForSubscrption);
                }
            }
        }


        protected virtual void RefreshIcons()
        {
            foreach (var icon in icons)
            {
                icon.sprite = GetIconSprite(currentType);
                icon.SetNativeSize();
            }
        }


        protected virtual void InitialRefresh() { }

        protected abstract void OnChooseCard();

        protected abstract Sprite GetIconSprite(T type);

        protected abstract bool IsBought(T type);

        #endregion



        #region Events handlers

        private void ChooseButton_OnClick() =>
            OnChooseCard();


        protected virtual void PurchaseSubscriptionButton_OnClick() =>
            GameServices.Instance.ProposalService.GetOffer<SubscriptionOffer>().ForcePropose();

        #endregion
    }
}
