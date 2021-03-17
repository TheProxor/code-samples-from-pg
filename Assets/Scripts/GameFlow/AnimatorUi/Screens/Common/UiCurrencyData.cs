using System;
using TMPro;
using UnityEngine;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiCurrencyData
    {
        public CurrencyType currencyType = default;
        public GameObject root = default;
        public TMP_Text text = default;
        public Transform icon = default;
    }
}