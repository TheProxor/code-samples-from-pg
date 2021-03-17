using System;
using Modules.InAppPurchase;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Ui.Settings
{
    [Serializable]
    public class StoreItemIconData
    {
        [ValueDropdown(nameof(AllKeys))]
        public string storeId = default;
        public GameObject rootGameObject = default;

        protected static string[] AllKeys => IAPs.Keys.AllKeys;
    }
}