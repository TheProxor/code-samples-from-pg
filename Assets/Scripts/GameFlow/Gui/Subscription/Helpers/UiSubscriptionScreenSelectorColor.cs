using System;
using Drawmasters.OffersSystem;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSubscriptionScreenSelectorColor : UiElementSelector<UiSubscriptionScreenSelectorColor.Data, UiOfferType>
    {
        [Serializable]
        public class Data : UiElementSelectorData<UiOfferType>
        {
            public Color color = Color.white;
        }

        [SerializeField] private Data[] data = default;
        [SerializeField] private Graphic[] graphics = default;


        protected override void OnSelect(UiOfferType offerType, Data data)
        {
            foreach (var graphic in graphics)
            {
                graphic.color = data.color;
            }
        }


        protected override Data FindData(UiOfferType offerType) =>
            Array.Find(data, e => e.key == offerType);
    }
}
