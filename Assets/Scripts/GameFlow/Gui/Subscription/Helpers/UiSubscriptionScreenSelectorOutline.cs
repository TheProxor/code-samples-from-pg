using System;
using UnityEngine;
using TMPro;
using Drawmasters.OffersSystem;

namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSubscriptionScreenSelectorOutline : UiElementSelector<UiSubscriptionScreenSelectorOutline.Data, UiOfferType>
    {
        [Serializable]
        public class Data : UiElementSelectorData<UiOfferType>
        {
            public Color outline = Color.white;
            public bool shouldColorUnderlay = true;
        }

        [SerializeField] private Data[] data = default;
        [SerializeField] private TMP_Text[] texts = default;


        protected override void OnSelect(UiOfferType ownerType, Data data)
        {
            foreach (var text in texts)
            {
                Material savedMaterial = text.fontMaterial;

                text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, data.outline);

                if (data.shouldColorUnderlay)
                {
                    text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, data.outline);
                }

                text.fontMaterial = new Material(savedMaterial);
            }
        }


        protected override Data FindData(UiOfferType ownerType) =>
            Array.Find(data, e => e.key == ownerType);
    }
}
