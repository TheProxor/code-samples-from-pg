using System;
using Drawmasters.OffersSystem;
using UnityEngine;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSubscriptionScreenSelectorObjects : UiElementSelector<UiSubscriptionScreenSelectorObjects.Data, UiOfferType>
    {
        [Serializable]
        public class Data : UiElementSelectorData<UiOfferType>
        {
            public GameObject[] objects = default;
        }

        [SerializeField] private Data[] data = default;


        protected override void OnSelect(UiOfferType offerType, Data selectedData)
        {
            foreach (var d in data)
            {
                CommonUtility.SetObjectsActive(d.objects, d.key == offerType);
            }
        }


        protected override Data FindData(UiOfferType offerType) =>
            Array.Find(data, e => e.key == offerType);
    }
}
