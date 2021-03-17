using System;
using Drawmasters.Mansion;
using UnityEngine;
using DG.Tweening;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;

namespace Drawmasters.Ui.Mansion
{
    public class UiMansionRoomObject : MonoBehaviour
    {
        #region Fields

        public event Action<UiMansionRoomObject> OnCompleted;

        public event Action<UiMansionRoomObject> OnTriedUpgrade;

        [SerializeField] private MansionRoomObjectType objectType = default;

        [Header("Progress")]

        [SerializeField] private RoomObjectAnimationHandler animationHandler = default;

        private int roomIndex;
        private int canvasOrder;
        private bool wasTutorialShow;

        //TODO dirty
        private bool wasCompleted;

        private UiMansionSwipe swipe;

        #endregion



        #region Properties

        public MansionRoomObjectType ObjectType =>
            objectType;


        public bool IsСompleted =>
            CompletedProgress >= 1.0f || WasRoomCompleted;

        public bool WasRoomCompleted =>
            GameServices.Instance.PlayerStatisticService.PlayerMansionData.WasRoomCompleted(roomIndex);

        public int CurrentUpdates =>
            GameServices.Instance.PlayerStatisticService.PlayerMansionData.FindMansionRoomObjectUpgrades(roomIndex, objectType);


        public int NeededUpdates =>
            IngameData.Settings.mansionRewardPackSettings.FindObjectTotalUpgrades(objectType);


        public float CompletedProgress
        {
            get
            {
                float result = CurrentUpdates / (float)NeededUpdates;

                if (result > 1f)
                {
                    CustomDebug.Log("Logical error of progress calculating.");
                }

                result = Mathf.Clamp01(result);

                return result;
            }
        }

        public Transform ButtonTransform =>
            animationHandler.hammerControl.hammerButton.transform;

        #endregion



        #region Methods

        public void Initialize(int _roomIndex, UiMansionSwipe _swipe, int _canvasOrder)
        {
            roomIndex = _roomIndex;
            canvasOrder = _canvasOrder;

            wasCompleted = IsСompleted;

            swipe = _swipe;
            swipe.OnSwipeBegin += Swipe_OnSwipeBegin;
            swipe.OnSwipeEnd += Swipe_OnSwipeEnd;

            Refresh();
        }


        public void Deinitialize()
        {
            swipe.OnSwipeBegin -= Swipe_OnSwipeBegin;
            swipe.OnSwipeEnd -= Swipe_OnSwipeEnd;

            if (animationHandler != null)
            {
                animationHandler.Deinitialize();
            }

            FinishTutorialFinger();

            DOTween.Kill(this);
        }


        public void InitializeButtons()
        {
            animationHandler.hammerControl.hammerButton.onClick.AddListener(UpgradeButton_OnClick);
        }


        public void DeinitializeButtons()
        {
            animationHandler.hammerControl.hammerButton.onClick.RemoveListener(UpgradeButton_OnClick);
        }


        public void StartTutorialFinger() =>
            CommonUtility.SetObjectActive(animationHandler.hammerControl.tutorialRoot, true);


        public void FinishTutorialFinger() =>
            CommonUtility.SetObjectActive(animationHandler.hammerControl.tutorialRoot, false);
        

        public void UpgradeObject()
        {
            GameServices.Instance.PlayerStatisticService.PlayerMansionData.UpgradeMansionRoomObject(roomIndex, objectType);

            if (IsСompleted)
            {
                PlayerMansionData playerMansionData = GameServices.Instance.PlayerStatisticService.PlayerMansionData;
                playerMansionData.FindMansionRoomData(roomIndex).UnmarkAllObjectsUpgradeHardEntered();

                OnCompleted?.Invoke(this);
            }

            if (animationHandler != null)
            {
                animationHandler.PlayUpgradeAnimation();
            }

            Refresh();
        }


        public void PlayShakeAnimation()
        {
            if (animationHandler != null)
            {
                animationHandler.PlayNonUpgradeAnimation();
            }
        }


        public void SetDisabledState()
        {
            if (animationHandler != null)
            {
                animationHandler.SetDisabledState();
            }
        }


        public void PlayShowAnimation()
        {
            if (animationHandler != null)
            {
                animationHandler.ShowHammer();
            }
        }


        public void ApplyCompletedState()
        {
            PlayerMansionData playerMansionData = GameServices.Instance.PlayerStatisticService.PlayerMansionData;
            if (playerMansionData.WasRoomCompleted(roomIndex))
            {
                animationHandler.ApplyRoomCompleteState();
            }
        }


        private void Refresh()
        {
            if (IsСompleted)
            {
                if (!wasCompleted)
                {
                    wasCompleted = true;

                    animationHandler.HideHammer();

                    animationHandler.PlayShowObjectsAnimation();
                }
                else
                {
                    animationHandler.SetHammerHidden();

                    animationHandler.SetObjectsShownState();
                }

                ApplyCompletedState();
            }

            animationHandler.hammerControl.hammerButton.enabled = !IsСompleted;
            animationHandler.hammerControl.fillImage.fillAmount = CompletedProgress; // TODO: via DOTween

            animationHandler.hammerControl.progressText.text = $"{CurrentUpdates}/{NeededUpdates}";
        }

        #endregion



        #region Events handlers

        private void UpgradeButton_OnClick()
        {
            OnTriedUpgrade?.Invoke(this);
        }


        private void Swipe_OnSwipeBegin()
        {
            if (!IsСompleted)
            {
                if (animationHandler != null)
                {
                    animationHandler.HideHammer();
                }
            }
        }


        private void Swipe_OnSwipeEnd()
        {
            if (!IsСompleted)
            {
                if (animationHandler != null)
                {
                    animationHandler.ShowHammer();
                }
            }
        }

        #endregion
    }
}
