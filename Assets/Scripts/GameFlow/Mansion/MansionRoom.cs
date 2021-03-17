using System;
using DG.Tweening;
using Drawmasters.ServiceUtil;
using Drawmasters.Mansion;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Utils;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Statistics.Data;
using System.Linq;
using Drawmasters.Effects;


namespace Drawmasters.Ui.Mansion
{
    public class MansionRoom : MonoBehaviour
    {
        #region Fields

        public event Action<RewardData, Action> OnShouldReceiveReward; // reward, callback


        public event Action OnCompleted; 
        public event Action OnOpened;

        [SerializeField] private RectTransform bodyRectTransform = default;
        [SerializeField] private Transform bottomGateFxRoot = default;
        [SerializeField] private CanvasGroup upgradableRoot = default;

        [SerializeField] private UiMansionRoomObject[] objectsData = default;
        [SerializeField] private UiMansionRoomShooter[] shootersData = default;

        [SerializeField] private Image progressFillImage = default;

        [SerializeField] private Material alphaGradient = default;
        [SerializeField] private Image glassImage = default;

        [SerializeField] private Animator mainAnimator = default;

        private RoomAnimationHandler roomAnimationHandler;

        private MansionRoomData roomData;
        private PlayerMansionData playerMansionData;

        private int index;
        private RealtimeTimer refreshTimer;

        private Material cachedAlpgaGradient;

        private UiMansionSwipe uiMansionSwipe;

        // TODO dirty
        bool wasOpened;

        #endregion



        #region Properties 

        public bool IsOpen =>
            GameServices.Instance.PlayerStatisticService.PlayerMansionData.WasRoomOpened(index);


        public bool IsCompleted =>
            RoomProgress >= 1.0f || WasRoomCompleted;

        public bool WasRoomCompleted =>
           GameServices.Instance.PlayerStatisticService.PlayerMansionData.WasRoomCompleted(index);

        public float RoomProgress
        {
            get
            {
                float totalNeedProgress = objectsData.Length;
                float totalExistProgress = 0;

                foreach (var data in objectsData)
                {
                    totalExistProgress += data.CompletedProgress;
                }

                return totalExistProgress / totalNeedProgress;
            }
        }


        public bool ShouldHardScroll { get; set; }


        public string MusicKey =>
            roomData.musicKey;

        #endregion



        #region Methods

        public void MarkTutorialMenuEnter()
        {
            var objectDataForTutorial = Array.Find(objectsData, e => e.ObjectType == IngameData.Settings.mansionRewardPackSettings.mansionRoomObjectTypeForTutorial);

            if (objectDataForTutorial != null)
            {
                objectDataForTutorial.StartTutorialFinger();
            }
        }


        public void Initialize(int _index, int _canvasOrder, UiMansionSwipe swipe)
        {
            // Position resets somehow. Hot fix to prevent reset. Vladislav.k
            Vector2 centerPivot = Vector2.one * 0.5f;
            bodyRectTransform.anchorMin = centerPivot;
            bodyRectTransform.anchorMax = centerPivot;
            bodyRectTransform.pivot = centerPivot;
            bodyRectTransform.anchoredPosition = Vector2.zero;

            uiMansionSwipe = swipe;

            index = _index;
            playerMansionData = GameServices.Instance.PlayerStatisticService.PlayerMansionData;
            roomData = IngameData.Settings.mansionRewardPackSettings.FindMansionRoomData(index);

            refreshTimer = GameServices.Instance.ProposalService.MansionRewardController.GetRefreshTimer(index);

            playerMansionData.FindMansionRoomData(index).WriteAvailableObjects(objectsData.Select(e => e.ObjectType).ToArray());

            foreach (var data in objectsData)
            {
                data.Initialize(index, swipe, _canvasOrder);
                data.OnCompleted += Object_OnCompleted;
                data.OnTriedUpgrade += Data_OnTriedUpgrade;
            }

            if (IsCompleted)
            {
                refreshTimer.Initialize();
            }

            if (roomData != null)
            {
                for (int i = 0; i < shootersData.Length; i++)
                {
                    shootersData[i].Initialize(IsCompleted, swipe);
                }
            }

            glassImage.material = alphaGradient;

            wasOpened = IsOpen;

            roomAnimationHandler = new RoomAnimationHandler(mainAnimator);

            FillRoomProgress(RoomProgress, RoomProgress, true);

            RefreshVisual();

            DisableUpgradableRoot();

            uiMansionSwipe.OnSwipeEnd += EnableUpgradableRoot;
        }

        public void Deinitialize()
        {
            foreach (var data in objectsData)
            {
                data.Deinitialize();
                data.OnCompleted -= Object_OnCompleted;
                data.OnTriedUpgrade -= Data_OnTriedUpgrade;
            }

            foreach (var i in shootersData)
            {
                i.Deinitialize();
            }

            uiMansionSwipe.OnSwipeEnd -= EnableUpgradableRoot;

            DOTween.Kill(this);
        }


        public bool TryFindReward(RewardData rewardData, out Vector3 pos)
        {
            pos = default;

            foreach (var data in objectsData)
            {
                CurrencyReward reward = IngameData.Settings.mansionRewardPackSettings.FindObjectReward(data.ObjectType);

                if (rewardData == reward)
                {
                    pos = data.ButtonTransform.position;
                    return true;
                }
            }

            return false;
        }


        private void RefreshVisual()
        {
            for (int i = 0; i < shootersData.Length; i++)
            {
                shootersData[i].Refresh(IsCompleted);
            }

            if (IsOpen)
            {
                if (IsCompleted)
                {
                    roomAnimationHandler.SetOpenedState();
                    roomAnimationHandler.SetCompletedState();
                }
                else
                {
                    roomAnimationHandler.SetOpenedState();
                }
            }
            else
            {
                foreach (var i in objectsData)
                {
                    i.SetDisabledState();
                }
            }
        }

        public void ClaimPassiveReward()
        {
            GameServices.Instance.ProposalService.MansionRewardController.MarkRewardApplied(index);
        }

        public void InitializeButtons()
        {
            foreach (var data in objectsData)
            {
                data.InitializeButtons();
            }
        }


        public void DeinitializeButtons()
        {
            foreach (var data in objectsData)
            {
                data.DeinitializeButtons();
            }
        }


        private void FillRoomProgress(float from, float to, bool isImmediately = false)
        {
            DOTween.Kill(this, true);

            if (isImmediately)
            {
                progressFillImage.fillAmount = from;
            }
            else
            {
                // TODO settings
                DOTween.To(() => from, factor => progressFillImage.fillAmount = factor, to, 0.3f)
                    .SetId(this);
            }
        }

        public void OpenRoom() =>
            playerMansionData.OpenRoom(index);
        

        public void PlayOpenRoomAnimation()
        {
            if (!wasOpened)
            {
                wasOpened = true;
                roomAnimationHandler.PlayOpenRoomAnimation();

                foreach (var i in objectsData)
                {
                    i.PlayShowAnimation();
                }

                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUICottageGateOpenBottomShine, bottomGateFxRoot.transform.position);
                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUICottageGateOpen, parent: bottomGateFxRoot, transformMode: TransformMode.Local);

                OnOpened?.Invoke();
            }
        }


        private void EnableUpgradableRoot() =>
            upgradableRoot.alpha = 1.0f;


        private void DisableUpgradableRoot() =>
            upgradableRoot.alpha = 0.0f;

        #endregion



        #region Events handlers

        private void Data_OnTriedUpgrade(UiMansionRoomObject upgradedObject)
        {
            IProposable proposable = new CurrencyProposal((1.0f, CurrencyType.MansionHammers));
            proposable.Propose((result) =>
            {
                if (result)
                {
                    float from = RoomProgress;

                    upgradedObject.UpgradeObject();

                    FillRoomProgress(from, RoomProgress);
                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUICottageHammerUse, parent: upgradedObject.ButtonTransform, transformMode: TransformMode.Local);

                    OpenRoom();
                }
                else
                {
                    upgradedObject.PlayShakeAnimation();
                }
            });


            foreach (var data in objectsData)
            {
                data.FinishTutorialFinger();
            }
        }


        private void Object_OnCompleted(UiMansionRoomObject sender)
        {
            CurrencyReward currencyReward = IngameData.Settings.mansionRewardPackSettings.FindObjectReward(sender.ObjectType);

            OnShouldReceiveReward?.Invoke(currencyReward, null);

            if (IsCompleted)
            {
                playerMansionData.MarkRoomCompleted(index);

                refreshTimer.Initialize();

                OnShouldReceiveReward?.Invoke(roomData.shooterSkinReward, () =>
                {
                    roomAnimationHandler.PlayCompleteRoomAnimation();

                    RefreshVisual();
                });

                foreach (var data in objectsData)
                {
                    data.ApplyCompletedState();
                }

                OnCompleted?.Invoke();
            }
            else
            {
                RefreshVisual();
            }
        }

        #endregion



        #region Editor methods

        [Sirenix.OdinInspector.Button]
        private void FillData()
        {
            objectsData = gameObject.GetComponentsInChildren<UiMansionRoomObject>(true);
            shootersData = gameObject.GetComponentsInChildren<UiMansionRoomShooter>(true);
        }

        #endregion
    }
}
