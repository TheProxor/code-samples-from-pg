using TMPro;
using UnityEngine;
using System;
using DG.Tweening;
using Drawmasters.Helpers;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Ui
{
    public class UiHudTop : MonoBehaviour
    {
        #region Nested types

        [Serializable]
        private class CurrencyData
        {
            public CurrencyType currencyType = default;
            public TMP_Text currencyCountText = default;
            public Transform currencyRoot = default;
            public Animator currencyAnimator = default;
            public TMP_Text announcerText = default;

            [NonSerialized]
            public float lastCurrencyCountUiValue = default;
        }

        #endregion



        #region Fields

        [Header("Data")]
        [SerializeField] private CurrencyData[] currencyData = default;

        [Header("Animation")]
        [SerializeField] private CanvasGroup canvasGroup = default;

        [SerializeField] private FactorAnimation defaultFactorAnimation = default;

        private PlayerCurrencyData currencyInfo;
        private bool isImmediately;
        private CurrencyType[] excludedTypes = Array.Empty<CurrencyType>();
        
        private UiHudCurrencyAmountController amountController;
        
        #endregion



        #region Properties

        public bool IsSubscribed { get; private set; }


        #endregion



        #region Public methods

        public void Initialize()
        {
            if (amountController == null)
            {
                amountController = new UiHudCurrencyAmountController(this);
            }

            amountController.Initialize();
        }


        public void Deinitialize()
        {
            amountController.Deinitialize();

            DOTween.Kill(this, true);
        }

        public void SetupExcludedTypes(params CurrencyType[] _excludedTypes)
            => excludedTypes = _excludedTypes;

        //HACK (need refactoring)
        public void InitializeCurrencyRefresh(bool isImmediatelyChange = false)
        {
            currencyInfo = GameServices.Instance.PlayerStatisticService.CurrencyData;
            
            currencyInfo.OnAnyCurrencyCountChanged += OnAnyCurrencyCountChanged;            

            IsSubscribed = true;

            //HACK
            if (!isImmediately)
            {
                isImmediately = isImmediatelyChange;
            }
        }

        public void DeinitializeCurrencyRefresh()
        {
            currencyInfo.OnAnyCurrencyCountChanged -= OnAnyCurrencyCountChanged;

            IsSubscribed = false;

            DOTween.Complete(this, true);
        }


        public void RefreshCurrencyVisual(float duration = 0.0f)
        {
            foreach (var data in currencyData)
            {
                bool isAvailable = data.currencyType.IsAvailableForShow();
                isAvailable &= !excludedTypes.Contains(i => data.currencyType == i);

                if (data.currencyRoot != null) // hack. prevent null ref from multiple reward (intermediate) in main menu
                {
                    CommonUtility.SetObjectActive(data.currencyRoot.gameObject, isAvailable);
                    UpdateCurrencyCount(data.currencyType, currencyInfo.GetEarnedCurrency(data.currencyType), duration);
                }
            }
        }

        public void RefreshCertainCurrencyVisual(CurrencyType type, float duration)
            => UpdateCurrencyCount(type, currencyInfo.GetEarnedCurrency(type), duration);


        private void OnAnyCurrencyCountChanged() =>
            RefreshCurrencyVisual(0.3f);


        public void PlayCurrencyAnimation(CurrencyType currencyType, string trigger)
        {
            CurrencyData data = FindCurrencyData(currencyType);
            if (data.currencyAnimator != null)
            {
                data.currencyAnimator.SetTrigger(trigger);
            }
        }

        public Transform FindCurrencyRootTransform(CurrencyType currencyType)
        {
            CurrencyData currencyData = FindCurrencyData(currencyType);
            return currencyData == null ? default : currencyData.currencyRoot;
        }


        public Transform FindCurrencyTextTransform(CurrencyType currencyType)
        {
            CurrencyData currencyData = FindCurrencyData(currencyType);
            return currencyData == null ? default : currencyData.currencyCountText.transform;
        }

        public void Show() => Show(defaultFactorAnimation);

        public void Hide() => Hide(defaultFactorAnimation);

        public void Show(FactorAnimation factorAnimation) => PlayFadeAnimation(factorAnimation, false);

        public void Hide(FactorAnimation factorAnimation) => PlayFadeAnimation(factorAnimation, true);

        #endregion



        #region Private methods

        private TMP_Text FindCurrencyText(CurrencyType currencyType)
        {
            CurrencyData currencyData = FindCurrencyData(currencyType);
            return currencyData == null ? default : currencyData.currencyCountText;
        }


        private float FindLastCurrencyCountUiValue(CurrencyType currencyType)
        {
            CurrencyData currencyData = FindCurrencyData(currencyType);
            return currencyData == null ? default : currencyData.lastCurrencyCountUiValue;
        }



        private CurrencyData FindCurrencyData(CurrencyType type)
        {
            CurrencyData result = Array.Find(currencyData, e => e.currencyType == type);

            if (result == null)
            {
                CustomDebug.Log($"No data found for currency type {type} in {this}");
            }

            return result;
        }


        private void UpdateCurrencyCount(CurrencyType currencyType, float value, float duration)
        {
            CurrencyType savedCurrencyType = currencyType;
            float lastCount = FindLastCurrencyCountUiValue(savedCurrencyType);
            TMP_Text text = FindCurrencyText(savedCurrencyType);

            DOTween.Complete(text, true);

            if (Mathf.Approximately(duration, 0.0f) || Mathf.Approximately(value, lastCount) || isImmediately)
            {
                FindCurrencyData(savedCurrencyType).lastCurrencyCountUiValue = value;
                text.text = value.ToShortFormat();
            }
            else
            {
                float savedLastCount = lastCount;

                CurrencyData currencyData = FindCurrencyData(savedCurrencyType);

                DOTween
                    .To(() => savedLastCount, SetData, value, duration)                    
                    .OnComplete(() => SetData(value))
                    .SetId(this)
                    .SetTarget(text);

                void SetData(float x)
                {
                    text.text = x.ToShortFormat();
                    currencyData.lastCurrencyCountUiValue = x;
                }
            }
        }

        private void PlayFadeAnimation(FactorAnimation factorAnimation, bool isReversed)
        {
            if (canvasGroup == null)
            {
                CustomDebug.Log("Canvas group on Hud Top is not assigned, but you are trying to use it!");
                return;
            }

            DOTween.Complete(this);
            factorAnimation.Play((value) => canvasGroup.alpha = value, this, isReversed : isReversed);
        }

        #endregion
    }
}