using System;
using System.Collections;
using System.Collections.Generic;
using Drawmasters.Levels;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui.Mansion
{
    public class UiMansion : RewardReceiveScreen
    {
        #region Fields

        [Header("Common")]
        [SerializeField] private Button backButton = default;
        [SerializeField] private UiMansionHammerPropose uiHammerPropose = default;
        [SerializeField] private UiMansionRewardPropose uiMansionRewardPropose = default;
        [SerializeField] private MansionRoom[] mansionRooms = default;
        [SerializeField] private UiMansionSwipe screenSwipe = default;
        [SerializeField] private ProposalAnimationHandler proposalAnimationHandler = default;

        [SerializeField] float shownDeltaWidth = default;

        private Coroutine expandRoutine;

        #endregion 



        #region Properties

        public override ScreenType ScreenType => ScreenType.Mansion;

        #endregion



        #region Methods

        public void MarkHardEnter(int indexToScroll, CurrencyType currencyType)
        {
            ScrollToRoom(indexToScroll);
            if (currencyType != CurrencyType.None)
            {
                uiHudTop.PlayCurrencyAnimation(currencyType, "Bounce");                
            }
        }
        

        public void MarkMenuEnter()
        {
            if (TryFindRoomIndexToScroll(out int index, true))
            {
                ScrollToRoom(index);
            }
        }


        public void MarkTutorialMenuEnter() =>
            mansionRooms.FirstObject().MarkTutorialMenuEnter();


        public override void Show()
        {
            base.Show();

            uiHudTop.SetupExcludedTypes(CurrencyType.RollBones);
            uiHudTop.InitializeCurrencyRefresh(true);
            uiHudTop.RefreshCurrencyVisual(0.0f);

            uiHammerPropose.Initialize();
            uiMansionRewardPropose.Initialize();

            uiMansionRewardPropose.OnShouldReceiveReward += UiMansion_OnShouldReceiveReward;
            uiMansionRewardPropose.OnClaimStart += UiMansionRewardPropose_OnClaimStart;
            uiMansionRewardPropose.OnClaimEnd += UiMansionRewardPropose_OnClaimEnd;
            uiHammerPropose.OnShouldReceiveReward += UiMansion_OnShouldReceiveReward;

            for (int i = 0; i < mansionRooms.Length; i++)
            {
                mansionRooms[i].Initialize(i, mainCanvas.sortingOrder, screenSwipe);
                mansionRooms[i].OnOpened += RefreshProposal;
                mansionRooms[i].OnShouldReceiveReward += UiMansion_OnShouldReceiveReward;
                mansionRooms[i].OnCompleted += Room_OnCompleted;
            }

            proposalAnimationHandler.Initialize(new List<MansionRoom>(mansionRooms), screenSwipe, uiHammerPropose, uiMansionRewardPropose);

            bool isForceProposed = GameServices.Instance.ProposalService.MansionProposeController.CanForcePropose;

            if (!mansionRooms.FirstObject().IsOpen)
            {
                mansionRooms.FirstObject().OpenRoom();
                mansionRooms.FirstObject().PlayOpenRoomAnimation();
            }

            // Logic for old users when rooms open was for keys
            int nextLockedRoomIndex = Array.FindIndex(mansionRooms, e => !e.IsOpen);
            bool isCorrectIndex = nextLockedRoomIndex != -1 && nextLockedRoomIndex != 0;
            if (isCorrectIndex && mansionRooms[nextLockedRoomIndex - 1].IsCompleted && !mansionRooms[nextLockedRoomIndex].IsOpen)
            {
                mansionRooms[nextLockedRoomIndex].OpenRoom();
                mansionRooms[nextLockedRoomIndex].PlayOpenRoomAnimation();
            }

            screenSwipe.Initialize();

            const float RoomReferenceResolutionHeight = 2208.0f;
            PlaySetupSpacingContentCoroutine(RoomReferenceResolutionHeight);
        }


        public override void Deinitialize()
        {
            screenSwipe.Deinitialize();

            uiHudTop.DeinitializeCurrencyRefresh();

            uiHammerPropose.Deinitialize();
            uiMansionRewardPropose.Deinitialize();

            uiHammerPropose.OnShouldReceiveReward -= UiMansion_OnShouldReceiveReward;
            uiMansionRewardPropose.OnShouldReceiveReward -= UiMansion_OnShouldReceiveReward;
            uiMansionRewardPropose.OnClaimStart -= UiMansionRewardPropose_OnClaimStart;
            uiMansionRewardPropose.OnClaimEnd -= UiMansionRewardPropose_OnClaimEnd;

            VisitAllRooms((room) =>
            {
                room.Deinitialize();

                room.OnShouldReceiveReward -= UiMansion_OnShouldReceiveReward;
                room.OnOpened -= RefreshProposal;
                room.OnCompleted -= Room_OnCompleted;
            });

            proposalAnimationHandler.Deinitialize();
            MonoBehaviourLifecycle.StopPlayingCorotine(expandRoutine);

            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            VisitAllRooms(r => r.InitializeButtons());

            uiHammerPropose.InitializeButtons();
            uiMansionRewardPropose.InitializeButtons();

            backButton.onClick.AddListener(BackButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            VisitAllRooms(r => r.DeinitializeButtons());

            uiHammerPropose.DeinitializeButtons();
            uiMansionRewardPropose.DeinitializeButtons();

            backButton.onClick.RemoveListener(BackButton_OnClick);
        }


        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        {
            Vector3 result = default;
            bool wasFound = default;

            VisitAllRooms((room) =>
            {
                if (room.TryFindReward(rewardData, out Vector3 pos))
                {
                    result = pos;
                    wasFound = true;
                }
            });

            if (!wasFound)
            {
                if (uiHammerPropose.TryFindReward(rewardData, out Vector3 posHammer))
                {
                    result = posHammer;
                    wasFound = true;
                }
                else if (uiMansionRewardPropose.TryFindReward(rewardData, out Vector3 pos))
                {
                    result = pos;
                    wasFound = true;
                }
            }

            if (!wasFound)
            {
                CustomDebug.Log($"Can't find result in {this} for {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            }

            return result;
        }


        private void VisitAllRooms(Action<MansionRoom> roomCallback)
        {
            foreach (var room in mansionRooms)
            {
                roomCallback?.Invoke(room);
            }
        }


        private bool TryFindRoomIndexToScroll(out int roomForScroll, bool isStart = false)
        {
            roomForScroll = mansionRooms.Length - 1;

            for (int i = 0; i < mansionRooms.Length; i++)
            {
                MansionRoom room = mansionRooms[i];

                bool needScroll = !room.IsOpen || !room.IsCompleted;
                if (!needScroll)
                {
                    bool canClaimPassiveIncome = !GameServices.Instance.ProposalService.MansionRewardController.IsTimerActive(i);

                    needScroll = canClaimPassiveIncome;
                }

                if (needScroll)
                {
                    roomForScroll = i;
                    break;
                }
            }

            bool allowScrolling;

            if (isStart)
            {
                allowScrolling = true;
            }
            else
            {
                allowScrolling = screenSwipe.CurrentScreen != roomForScroll;
                allowScrolling &= GameServices.Instance.ProposalService.MansionRewardController.IsTimerActive(screenSwipe.CurrentScreen);
            }

            return allowScrolling;
        }


        private void ScrollToRoom(int indexToScroll)
        {
            // HACK
            screenSwipe.TriggerDragBegin();
            screenSwipe.GoToScreen(indexToScroll);
        }


        private void PlaySetupSpacingContentCoroutine(float elementsWidth)
        {
            expandRoutine = MonoBehaviourLifecycle.PlayCoroutine(SetupElemenetsSpacing(mainCanvas.scaleFactor));

            IEnumerator SetupElemenetsSpacing(float _canvasScaleFactor)
            {
                yield return new WaitForEndOfFrame();

                float space = ((Screen.height / _canvasScaleFactor - elementsWidth) * 0.5f) + 2.0f * shownDeltaWidth;

                screenSwipe.Spacing = -space;
            }
        }

        #endregion



        #region Events handlers

        private void UiMansionRewardPropose_OnClaimStart(int roomIndex)
        {
            if (roomIndex >= 0 && roomIndex < mansionRooms.Length)
            {
                MansionRoom room = mansionRooms[roomIndex];

                room.ClaimPassiveReward();        
            }
        }

        private void UiMansionRewardPropose_OnClaimEnd(int roomIndex)
        {
            if (roomIndex >= 0 && roomIndex < mansionRooms.Length)
            {
                if (TryFindRoomIndexToScroll(out int index))
                {
                    ScrollToRoom(index);
                }
            }
        }

        private void BackButton_OnClick()
        {
            DeinitializeButtons();

            FromLevelToLevel.PlayTransition(() =>
            {
                HideImmediately();

                LevelsManager.Instance.UnloadLevel();
                LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, isForceHideIfExist: true);

                GameServices.Instance.MusicService.InstantRefreshMusic();
            });
        }


        private void UiMansion_OnShouldReceiveReward(RewardData reward, Action callback)
        {
            OnShouldApplyReward(reward, callback);
        }


        private void RefreshProposal()
        {
            MansionRoom room = mansionRooms[screenSwipe.CurrentScreen];

            uiMansionRewardPropose.SetEnabled(room.IsOpen && room.IsCompleted);
            uiHammerPropose.SetEnabled(room.IsOpen && !room.IsCompleted);

            uiMansionRewardPropose.SetupRoom(screenSwipe.CurrentScreen);
        }


        private void Room_OnCompleted()
        {
            MansionRoom nextLockedRoom = Array.Find(mansionRooms, e => !e.IsOpen);

            if (nextLockedRoom != null)
            {
                nextLockedRoom.OpenRoom();
                nextLockedRoom.ShouldHardScroll = true;
            }

            if (TryFindRoomIndexToScroll(out int index))
            {
                ScrollToRoom(index);
            }
        }
        
        
        private void UiMansion_OnShouldReceiveReward(RewardData reward)
        {
            OnShouldApplyReward(reward);
        }

        #endregion
    }
}
