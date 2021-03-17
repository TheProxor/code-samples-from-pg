using System;
using UnityEngine;
using UnityEngine.UI;
using SelectorKey = Drawmasters.Ui.UiLeagueLeaderBoardElementSelectorKey;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiLeagueLeaderBoardElementSelectorColor : UiElementSelector<UiLeagueLeaderBoardElementSelectorColor.Data, SelectorKey>
    {
        [Serializable]
        public class Data : UiElementSelectorData<SelectorKey>
        {
            public Color color = Color.white;
        }

        [SerializeField] private Data[] data = default;
        [SerializeField] private Graphic[] graphics = default;


        protected override void OnSelect(SelectorKey ownerType, Data data)
        {
            foreach (var gr in graphics)
            {
                gr.color = data.color;
            }
        }


        protected override Data FindData(SelectorKey ownerType) =>
            Array.Find(data, e => e.key.IsEquals(ownerType));
    }
}
