using System;
using DG.Tweening;
using Modules.General.Abstraction;
using Modules.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Modules.Advertising;


namespace Drawmasters.Ui
{
    public class RewardedVideoButton : TrackableMonoBehaviour
    {
        #region Nested types

        private enum State
        {
            Idle = 0,
            Loading = 1
        }

        #endregion


        #region Fields

        public event Action<AdActionResultType> OnVideoShowEnded;
        public event Action OnClick;

        private const int TicksCount = 9;

        [Header("Common")]
        [SerializeField] private Button button = default;
        [SerializeField] private GameObject loadingStateRoot = default;
        [SerializeField] private GameObject normalStateRoot = default;

        [Header("Loading state")]
        [SerializeField] private float rotateDuration = 1.0f;

        [SerializeField] private Transform loadingCircle = default;
        [SerializeField] private TMP_Text counterText = default;

        private string placement;
        private AdModule adModule;

        private State state;

        private bool isTimerActive;

        #endregion



        #region Properties

        public string Placement => placement;

        private bool CanShowVideo
        {
            get
            {
                bool result = true;

                result &= ReachabilityHandler.Instance.NetworkStatus != NetworkStatus.NotReachable;
                result &= AdvertisingManager.Instance.IsAdModuleByPlacementAvailable(adModule, placement);

                return result;
            }
        }

        #endregion



        #region Unity lifecycle

        // hotfix
        private void Update()
        {
            if (isTimerActive)
            {
                if (CanShowVideo)
                {
                    isTimerActive = false;

                    ShowVideo();
                }
            }
        }

        #endregion



        #region Methods

        public void Initialize(string _placement, AdModule _adModule = AdModule.RewardedVideo)
        {
            placement = _placement;
            adModule = _adModule;

            SetNormalStateActive(true);

            isTimerActive = false;
            
            InnerInitialize();
        }


        public void Deinitialize()
        {
            isTimerActive = false;

            #warning WRONG QUEUE. BUTTON BECOMES WITH LISTENER ON DEINIT. CHANGE AFTER SL2
            DeinitializeButtons();

            CancelAdShowRequest();

            DOTween.Kill(this);
            
            InnerDeinitialize();
        }


        public void InitializeButtons()
        {
            button.onClick.AddListener(Button_OnClick);
        }


        public void DeinitializeButtons()
        {
            button.onClick.RemoveListener(Button_OnClick);
        }


        public void CancelAdShowRequest()
        {
            SetNormalStateActive(true);

            DeinitializeButtons();
            InitializeButtons();
        }


        private void ShowLoadingState()
        {
            // TODO: hotfix
            if (state == State.Loading)
            {
                return;
            }

            DOTween.Kill(this);

            SetNormalStateActive(false);

            loadingCircle
                .DORotate(new Vector3(0.0f, 0.0f, 360.0f), rotateDuration, RotateMode.FastBeyond360)
                .SetId(this)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);

            counterText.text = TicksCount.ToString();
            DOTween.To(() => TicksCount, 
                        x => counterText.text = x.ToString(), 0, TicksCount)
                .SetId(this)
                .OnComplete(() => counterText.text = string.Empty);
        }


        private void SetNormalStateActive(bool active)
        {
            CommonUtility.SetObjectActive(normalStateRoot, active);
            CommonUtility.SetObjectActive(loadingStateRoot, !active);

            state = active ? State.Idle : State.Loading;
        }


        private void ShowVideo()
        {
            AdvertisingManager.Instance.TryShowAdByModule(adModule, placement, result =>
            {
                if (this.IsNull())
                {
                    CustomDebug.Log("Rewarded video button is null on showed callback");
                    return;
                }

                switch (result)
                {
                    case AdActionResultType.Success:
                    case AdActionResultType.Skip:
                        SetNormalStateActive(true);
                        InitializeButtons();

                        isTimerActive = false;
                        break;

                    case AdActionResultType.NotAvailable:
                        DeinitializeButtons();
                        ShowLoadingState();

                        isTimerActive = true;
                        break;

                    case AdActionResultType.NoInternet:
                        UiScreenManager.Instance.ShowScreen(ScreenType.NoInternet);
                        InitializeButtons();
                        break;

                    default:
                        InitializeButtons();
                        break;
                }

                OnVideoShowEnded?.Invoke(result);
            });
        }

        #endregion



        #region Events handlers

        private void Button_OnClick()
        {
            OnClick?.Invoke();
            
            DeinitializeButtons();
            ShowVideo();
        }

        #endregion
    }
}
