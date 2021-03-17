using System;
using UnityEngine;
using TMPro;
using SelectorKey = Drawmasters.Ui.UiLeagueLeaderBoardElementSelectorKey;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiLeagueLeaderBoardElementSelectorOutline : UiElementSelector<UiLeagueLeaderBoardElementSelectorOutline.Data, SelectorKey>
    {
        [Serializable]
        public class Data : UiElementSelectorData<SelectorKey>
        {
            public Color outline = Color.white;
            public bool shouldColorUnderlay = true;
        }

        [SerializeField] private Data[] data = default;
        [SerializeField] private TMP_Text[] texts = default;


        protected override void OnSelect(SelectorKey ownerType, Data data)
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


        protected override Data FindData(SelectorKey ownerType) =>
            Array.Find(data, e => e.key.IsEquals(ownerType));
    }
}
