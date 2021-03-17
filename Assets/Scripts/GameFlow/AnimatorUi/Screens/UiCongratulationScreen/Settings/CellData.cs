using System;
using TMPro;
using UnityEngine;


namespace Drawmasters.Ui.Settings
{
    [Serializable]
    public class CellData
    {
        public CurrencyType currencyType = default;
        public GameObject rootGameObject = default;
        public TMP_Text countText = default;
    }
}