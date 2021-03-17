using System;
using System.Collections.Generic;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class ModeButton : MonoBehaviour, IInitializable, IDeinitializable
    {
        #region Fields

        private const string IdleTrigger = "Idle";
        private const string UnlockModeTrigger = "UnlockMode";

        public event Action<ModeButton> OnModeButtonClick;

        [SerializeField] private RectTransform root = default;

        [SerializeField] private RectTransform scaleRoot = default;

        [SerializeField] private Button playButton = default;
        [SerializeField] private GameMode mode = default;

        [SerializeField] private List<GameObject> enabledObjects = default;
        [SerializeField] private List<GameObject> disabledObjects = default;

        [Header("Disabled")]
        [SerializeField] private TextMeshProUGUI unlockAmountText = default;
        [SerializeField] private Image unlockImage = default;
        [SerializeField] private Image unlockShadowImage = default;

        [Header("Enabled")]
        [SerializeField] private TextMeshProUGUI levelAmountText = default;

        [Header("Visual")]
        [SerializeField] private string uiPlankNameText = default;

        private bool isForceLocked;
        private Animator mainAnimator;

        private IPlayerStatisticService playerStatisticService;

        #endregion



        #region Properties

        public GameMode Mode => mode;

        public RectTransform Root => root;

        public RectTransform ScaleRoot => scaleRoot;

        public string UiPlankNameText => uiPlankNameText;

        public Button PlayButton => playButton;

        #endregion



        #region IInitializable

        public void Initialize()
        {
            mainAnimator = GetComponent<Animator>();
            mainAnimator.SetTrigger(IdleTrigger);
            playButton.onClick.AddListener(PlayButton_OnClick);

            playerStatisticService = GameServices.Instance.PlayerStatisticService;

            RefreshVisual();
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            playButton.onClick.RemoveListener(PlayButton_OnClick);
        }

        #endregion



        #region Methods

        public void PlayUnlockAnimation() =>
            mainAnimator.SetTrigger(UnlockModeTrigger);

        public void RefreshVisual()
        {
            if (mode.IsExcludedFromLoad())
            {
                return;
            }

            bool isModeOpen = playerStatisticService.ModesData.IsModeOpen(mode);
            bool isEnabled = isModeOpen && !isForceLocked;

            enabledObjects.ForEach(go => CommonUtility.SetObjectActive(go, isEnabled));
            disabledObjects.ForEach(go => CommonUtility.SetObjectActive(go, !isEnabled));

            int index = playerStatisticService.PlayerData.GetModeCurrentIndex(mode);

            if (levelAmountText != null)
            {
                levelAmountText.text = (index + 1).ToString();
            }

            bool isNextLockedMode = playerStatisticService.ModesData.NextLockedMode == Mode;

            float progress = isModeOpen ? 1.0f : 0.5f;// playerStatisticService.ChaptersData.ModeProgress(Mode);

            bool showShouldProgressFill = isNextLockedMode || (isModeOpen && isForceLocked);

            if (unlockImage != null)
            {
                unlockImage.fillAmount = showShouldProgressFill ? progress : 0.0f;
            }

            if (unlockShadowImage != null)
            {
                unlockShadowImage.fillAmount = showShouldProgressFill ? progress : 0.0f;
            }

            if (unlockAmountText != null)
            {
                unlockAmountText.text = string.Concat(progress.ToPercents(), "%");
                CommonUtility.SetObjectActive(unlockAmountText.gameObject, showShouldProgressFill);
            }
        }


        public void SetButtonEnabled(bool enable) =>
            playButton.interactable = enable;

        public void SetButtonLocked(bool locked)
        {
            isForceLocked = locked;

            RefreshVisual();
        }

        #endregion



        #region Events handlers

        private void PlayButton_OnClick()
            => OnModeButtonClick?.Invoke(this);

        // Called from unity animator
        private void PlayUnlockFx()
        {
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIMenuNewModeOpen, parent: ScaleRoot, transformMode: TransformMode.Local);
            SetButtonLocked(false);
        }

        #endregion
    }
}

