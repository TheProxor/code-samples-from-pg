using TMPro;
using Modules.Sound;
using DG.Tweening;
using Drawmasters.Effects;
using Drawmasters.Utils;
using Drawmasters.Ui;
using Drawmasters.Utils.Ui;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;
using Drawmasters.Announcer;
using Modules.Analytics;
using Modules.General.Abstraction;
using Object = UnityEngine.Object;


namespace Drawmasters.Proposal
{
    public abstract class UiSeasonEventRewardElement : MonoBehaviour, IDeinitializable, IScrollButton, IUiOverlayTutorialObject
    {
        #region Nested types

        public enum State
        {
            None                     = 0,
            Claimed                  = 1,
            ReadyToClaim             = 2,
            NotReached               = 3,
            CanClaimForAds           = 4,
            ReachedCanNotClaim       = 5,
            ReachedCanNotClaimForAds = 6,
        }

        #endregion



        #region Fields

        public event Action<UiSeasonEventRewardElement> OnShouldReceiveReward;

        private const float ScaleIconMultiplier = 0.76f;

        private static readonly HashSet<RewardType> ScaleIconMultiplierTypes = new HashSet<RewardType>()
        {
            RewardType.ShooterSkin,
            RewardType.PetSkin
        };


        [SerializeField] protected Transform body = default;
        [SerializeField] protected Button receiveRewardButton = default;
        [SerializeField] private Button notReachedRewardButton = default;

        [SerializeField] private Image icon = default;
        [SerializeField] private Image backImage = default;

        [SerializeField] private TMP_Text textInfo = default;

        [SerializeField] private CanvasGroup claimedIconCanvasGroup = default;
        [SerializeField] private CanvasGroup lockIconCanvasGroup = default;
        [SerializeField] private Animator animator = default;

        [Header("Effects")]
        [SerializeField] private Transform iconFxTransform = default;
        [SerializeField] private Transform unlockFxRoot = default;
        [SerializeField] private IdleEffect backIdleEffect = default;
        [SerializeField] private IdleEffect rewardIdleEffect = default;

        [Header("Tutorial. Not required")]
        [SerializeField] private GameObject overlayTutorialObject = default;
        [SerializeField] private GameObject tutorialRoot = default;
        
        [Header("Elements")]
        [SerializeField] private UiSeasonEventRewardElementReachedPlank reachedPlank = default;

        protected SeasonEventVisualSettings visualSettings;
        private SeasonEventSettings settings;

        private MonopolyCurrencyAnnouncer announcer;
        private SeasonEventProposeController controller;

        protected int localIndex;

        #endregion



        #region Properties

        public State CurrentState { get; private set; }

        public Transform IconCurrencyTransform => iconFxTransform;

        public RewardData RewardData { get; private set; }

        public bool IsClaimed =>
            CurrentState == State.Claimed;

        public abstract SeasonEventRewardType SeasonEventRewardType { get; }

        protected abstract bool ShouldSetBackNativeSize { get; }

        protected abstract Vector3 UnlockFxScale { get; }

        protected virtual List<CanvasGroup> AllIconsGraphic =>
            new List<CanvasGroup> { claimedIconCanvasGroup, lockIconCanvasGroup, reachedPlank.CanvasGroup };


        private bool IsReachedCanNotClaim =>
            CurrentState == State.ReachedCanNotClaim ||
            CurrentState == State.ReachedCanNotClaimForAds;

        #endregion



        #region Unity lifecycle

        // This fixes strange animator behaviour - sometimes SetTrigger won't work without that
        private void Awake()
        {
            animator.keepAnimatorControllerStateOnDisable = true;
            receiveRewardButton.animator.keepAnimatorControllerStateOnDisable = true;
        }

        #endregion



        #region Methods

        public virtual void Initialize(SeasonEventProposeController _controller)
        {
            controller = _controller;
            reachedPlank.Initialize();

            // to reset because of SetFxKey logic below
            backIdleEffect.SetFxKey(string.Empty);
            rewardIdleEffect.SetFxKey(string.Empty);
        }


        public virtual void Deinitialize()
        {
            SetTutorialEnabled(false);
            DestroyAnnouncer();

            backIdleEffect.StopEffect();
            rewardIdleEffect.StopEffect();

            reachedPlank.Deinitialize();
            DOTween.Kill(this);
            CurrentState = default;


            void DestroyAnnouncer()
            {
                if (announcer != null)
                {
                    announcer.Deinitialize();
                    Object.Destroy(announcer.gameObject);
                    announcer = null;

                }
            }
        }
        

        public virtual void InitializeButtons()
        {
            notReachedRewardButton.onClick.AddListener(NotReachedRewardButton_OnClick);
            receiveRewardButton.onClick.AddListener(ReceiveRewardButton_OnClick);
        }


        public virtual void DeinitializeButtons()
        {
            notReachedRewardButton.onClick.RemoveListener(NotReachedRewardButton_OnClick);
            receiveRewardButton.onClick.RemoveListener(ReceiveRewardButton_OnClick);
        }


        public void SetupReward(RewardData rewardData)
        {
            RewardData = rewardData;

            visualSettings = IngameData.Settings.seasonEvent.seasonEventVisualSettings;
            settings = IngameData.Settings.seasonEvent.seasonEventSettings;
        }


        public void SetupIndex(int _localIndex) =>
            localIndex = _localIndex;


        public virtual void SetState(State _state, bool isImmediately)
        {
            bool wasStateChanged = CurrentState != _state;
            CurrentState = _state;

            SetVisualState(CurrentState, isImmediately, wasStateChanged);
        }


        private void SetVisualState(State state, bool isImmediately, bool wasStateChanged)
        {
            bool canClaim = state == State.ReadyToClaim || state == State.CanClaimForAds;
            receiveRewardButton.interactable = canClaim || IsReachedCanNotClaim;
            notReachedRewardButton.interactable = state == State.NotReached;
            CommonUtility.SetObjectActive(notReachedRewardButton.gameObject, state == State.NotReached);

            string trigger = canClaim ? AnimationKeys.SeasonEvent.CanClaim : AnimationKeys.SeasonEvent.Idle;
            animator.SetTrigger(trigger);

            RefreshVisual();
            SetIconsVisual();
            SetBackgroundVisual();

            textInfo.outlineColor = visualSettings.GetRewardTextOutlineColor(SeasonEventRewardType, state);

            if (isImmediately)
            {
                textInfo.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, textInfo.outlineColor);
            }
            else
            {
                textInfo.materialForRendering.SetColor(ShaderUtilities.ID_UnderlayColor, textInfo.outlineColor);
            }

            reachedPlank.SetState(state, isImmediately, wasStateChanged);

            Material mGlowActionScoreMaterial = textInfo.fontMaterial;
            textInfo.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, textInfo.outlineColor);
            textInfo.fontMaterial = new Material(mGlowActionScoreMaterial);


            void SetIconsVisual()
            {
                if (wasStateChanged)
                {
                    CanvasGroup iconGraphicsForShow = GetIconsGraphic(CurrentState);
                    List<CanvasGroup> otherIconImages = AllIconsGraphic.Where(e => e != iconGraphicsForShow && e.alpha > 0.0f).ToList();

                    if (isImmediately)
                    {
                        iconGraphicsForShow.alpha = 1.0f;
                        otherIconImages.ForEach(e => e.alpha = 0.0f);
                    }
                    else
                    {
                        bool shouldSkipAnimation = iconGraphicsForShow == reachedPlank.CanvasGroup && reachedPlank.CanvasGroup.alpha > 0.0f;
                        if (!shouldSkipAnimation)
                        {
                            visualSettings.iconShowAlphaAnimation.Play((value) => iconGraphicsForShow.alpha = value, this);
                            visualSettings.iconShowScaleAnimationIn.Play((value) => iconGraphicsForShow.transform.localScale = value, this,
                                () => visualSettings.iconShowScaleAnimationOut.Play((value) => iconGraphicsForShow.transform.localScale = value, this, isReversed: true));
                        }

                        visualSettings.iconHideAlphaAnimation.Play((value) => otherIconImages.ForEach(e => e.alpha = value), this, isReversed: true);
                    }
                }
            }


            void SetBackgroundVisual()
            {
                Sprite targetBackSprite = visualSettings.GetRewardBackSprite(SeasonEventRewardType, state);
                backImage.sprite = targetBackSprite;

                if (ShouldSetBackNativeSize)
                {
                    backImage.SetNativeSize();
                }

                if (wasStateChanged)
                {
                    string backgroundFxKey = default;

                    if (SeasonEventRewardType == SeasonEventRewardType.Pass || SeasonEventRewardType == SeasonEventRewardType.Main)
                    {
                        if (state == State.CanClaimForAds ||
                            state == State.ReadyToClaim)
                        {
                            backgroundFxKey = EffectKeys.FxGUISeasonPassActiveRewardBackShine;
                        }
                        else if (state != State.Claimed)
                        {
                            backgroundFxKey = EffectKeys.FxGUISeasonPassRewardBackShine;
                        }
                    }

                    if (!backIdleEffect.IsKeyEquals(backgroundFxKey))
                    {
                        backIdleEffect.StopEffect();
                        backIdleEffect.SetFxKey(backgroundFxKey);
                        backIdleEffect.CreateAndPlayEffect();
                    }

                    string idleFxkey = default;

                    bool isNextStageIndex = visualSettings.FindIfLevelElementNextStage(localIndex) ||
                                            localIndex == controller.MaxLevelIndexWithoutMainAndBonus;

                    if (state != State.Claimed &&
                        (SeasonEventRewardType == SeasonEventRewardType.Pass || 
                         SeasonEventRewardType == SeasonEventRewardType.Main) &&
                        isNextStageIndex)
                    {
                        idleFxkey = EffectKeys.FxGUISeasonEventBestRewardShine;
                    }

                    if (!rewardIdleEffect.IsKeyEquals(idleFxkey))
                    {
                        rewardIdleEffect.StopEffect();
                        rewardIdleEffect.SetFxKey(idleFxkey);
                        rewardIdleEffect.CreateAndPlayEffect();
                    }
                }
            }
        }


        private void RefreshVisual()
        {
            string infoText = string.Empty;

            switch (RewardData)
            {
                case CurrencyReward currency:
                    infoText = currency.UiPlusRewardText;
                    break;

                case WeaponSkinReward weapon:
                case ShooterSkinReward shooter:
                case PetSkinReward pet:
                case ForcemeterReward forcemeter:
                case SpinRouletteReward spinRoulette:
                    infoText = string.Empty;
                    break;

                default:
                    CustomDebug.Log($"Not implemented logic for {RewardData}");
                    break;
            }

            icon.sprite = GetRewardIconSprite(CurrentState, RewardData);
            textInfo.text = infoText;
            icon.SetNativeSize();

            RewardType rewardType = RewardData?.Type ?? default;
            
            bool shouldUseScaleMultiplier = ScaleIconMultiplierTypes.Contains(rewardType);
            
            icon.transform.localScale = shouldUseScaleMultiplier ? 
                Vector3.one * ScaleIconMultiplier : 
                Vector3.one;
        }


        public void PlayClaimFx()
        {
            EffectHandler handler = EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUISeasonPassCellUnlock, 
                unlockFxRoot.position, 
                unlockFxRoot.rotation, 
                unlockFxRoot);
            
            if (handler != null)
            {
                handler.transform.localScale = UnlockFxScale;
            }

            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIDone, 
                claimedIconCanvasGroup.transform.position, 
                claimedIconCanvasGroup.transform.rotation, 
                claimedIconCanvasGroup.transform);
            
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.CHESTOPEN);
        }



        public void PlayLockDestroyFx()
        {
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUISeasonPassLockDestroy, 
                iconFxTransform.position, 
                iconFxTransform.rotation, 
                iconFxTransform);

            SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.CHALLENGE_WHITE_LOCKS_OPEN);
        }


        public void SetTutorialEnabled(bool enabled)
        {
            if (tutorialRoot != null)
            {
                CommonUtility.SetObjectActive(tutorialRoot, enabled);
            }
        }


        public virtual void SetButtonAnimationEnabled(bool enabled) =>
            animator.SetBool("AnimationLock", !enabled);
        

        protected virtual CanvasGroup GetIconsGraphic(State state)
        {
            bool shouldShowLock = CurrentState == State.NotReached;
            bool shouldShowClaimed = CurrentState == State.Claimed;
            bool shouldShowPlank = CurrentState == State.CanClaimForAds ||
                                   CurrentState == State.ReachedCanNotClaim ||
                                   CurrentState == State.ReachedCanNotClaimForAds ||
                                   CurrentState == State.ReadyToClaim;

            CanvasGroup result = default;

            if (shouldShowLock)
            {
                result = lockIconCanvasGroup;
            }
            else if (shouldShowClaimed)
            {
                result = claimedIconCanvasGroup;
            }
            else if (shouldShowPlank)
            {
                result = reachedPlank.CanvasGroup;
            }

            return result;
        }


        protected Sprite GetRewardIconSprite(State state, RewardData data)
        {
            Sprite result = default;

            if (data is CurrencyReward currencyReward)
            {
                result = visualSettings.FindCurrencyIcon(SeasonEventRewardType, currencyReward.currencyType, state);
            }
            else if (data is PetSkinReward petSkinReward)
            {
                result = visualSettings.FindPetSkinIcon(SeasonEventRewardType, petSkinReward.skinType, state);
            }
            else if (data is WeaponSkinReward weaponSkinReward)
            {
                result = visualSettings.FindWeaponSkinIcon(weaponSkinReward.skinType);
            }
            else if (data is ShooterSkinReward shooterSkinReward)
            {
                result = visualSettings.FindShooterSkinIcon(shooterSkinReward.skinType);
            }
            else if (data is ForcemeterReward)
            {
                result = visualSettings.FindForcemeterRewardIcon(SeasonEventRewardType, state);
            }
            else if (data is SpinRouletteCashReward)
            {
                result = visualSettings.FindSpinRouletteCashRewardIcon(SeasonEventRewardType, state);
            }
            else if (data is SpinRouletteSkinReward)
            {
                result = visualSettings.FindSpinRouletteSkinRewardIcon(SeasonEventRewardType, state);
            }
            else if (data is SpinRouletteWaiponReward)
            {
                result = visualSettings.FindSpinRouletteWeaponRewardIcon(SeasonEventRewardType, state);
            }
            else
            {
                CustomDebug.Log($"Not implemented logic for {RewardData.Type}");
            }

            return result;
        }


        private void AttemptCreateAnnouncer()
        {
            if (announcer == null)
            {
                announcer = Instantiate(settings.canNotClaimAnnouncerPrefab,
                    transform.position,
                    transform.rotation,
                    body);
            }

            announcer.SetupData(visualSettings.FindAnnouncerData(SeasonEventRewardType));
        }


        private void AttemptPlayAnnouncer()
        {
            if (!announcer.IsTweenActive)
            {
                Vector3 offset = visualSettings.FindAnnouncerOffset(SeasonEventRewardType);
                announcer.PlayLocal(Vector3.zero, offset);
            }
        }

        #endregion



        #region Events handlers

        private void ReceiveRewardButton_OnClick()
        {
            if (IsReachedCanNotClaim)
            {
                AttemptCreateAnnouncer();
                announcer.SetupValues(visualSettings.FindAnnouncerBackgroundSprite(SeasonEventRewardType), settings.canNotClaimAnnouncerKey);
                AttemptPlayAnnouncer();

                PointerEventData pointerEventData = EventSystemController.CurrentPointerEventData;
                pointerEventData.selectedObject = receiveRewardButton.gameObject;
                OnPointerUp(pointerEventData);
            }
            else if (CurrentState == State.ReadyToClaim)
            {
                ReceiveReward();
            }
        }


        private void NotReachedRewardButton_OnClick()
        {
            if (controller.IsLockedByGoldenTicket(SeasonEventRewardType, localIndex))
            {
                AttemptCreateAnnouncer();
                announcer.SetupValues(visualSettings.FindAnnouncerBackgroundSprite(SeasonEventRewardType), settings.buyGoldenTicketAnnouncerKey);
                AttemptPlayAnnouncer();
            }

            animator.SetTrigger(AnimationKeys.SeasonEvent.Lock);
        }


        protected virtual void MarkRewardReceived(bool isRewardedVideo = false)
        {
            if (RewardData != null &&
                isRewardedVideo)
            {
                CommonEvents.SendAdVideoReward(RewardData.RewardText);
            }
        }

        protected void OnAdsWasWatched(AdModule module, AdActionResultType resultType)
        {
            bool isRewardedVideo = module == AdModule.RewardedVideo;
            
            bool wasAdsSuccess = isRewardedVideo ?
                resultType == AdActionResultType.Success : 
                resultType == AdActionResultType.Success || resultType == AdActionResultType.Skip;

            if (wasAdsSuccess)
            {
                ReceiveReward(isRewardedVideo);
            }
        }

        protected void ReceiveReward(bool isRewardedVideo = false)
        {
            MarkRewardReceived(isRewardedVideo);

            OnShouldReceiveReward?.Invoke(this);
        }

        #endregion



        #region IScrollButton

        public RectTransform ButtonRectTransform =>
            receiveRewardButton.transform as RectTransform;


        public Rect GetButtonWorldRect(Vector3 scale) =>
             CommonUtility.GetWorldRect(ButtonRectTransform, scale);


        public void OnPointerUp(PointerEventData pointerEventData) =>
            PressButtonUtility.ImmitateButtonUp(receiveRewardButton, pointerEventData);
        

        public void OnShouldClick(PointerEventData pointerEventData) =>
            PressButtonUtility.ImmitateButtonClick(receiveRewardButton, pointerEventData);

        #endregion



        #region IUiOverlayTutorialObject

        public GameObject OverlayTutorialObject => overlayTutorialObject;


        // hotfix decision cuz of TMP internal bug
        public void RefreshText()
        {
            CommonUtility.SetObjectActive(textInfo.gameObject, false);
            CommonUtility.SetObjectActive(textInfo.gameObject, true);
        }

        #endregion
    }
}
