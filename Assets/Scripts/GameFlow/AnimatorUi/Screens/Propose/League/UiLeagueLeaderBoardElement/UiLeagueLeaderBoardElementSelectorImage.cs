using System;
using UnityEngine;
using UnityEngine.UI;
using SelectorKey = Drawmasters.Ui.UiLeagueLeaderBoardElementSelectorKey;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiLeagueLeaderBoardElementSelectorImage : UiElementSelector<UiLeagueLeaderBoardElementSelectorImage.Data, SelectorKey>
    {
        [Serializable]
        public class Data : UiElementSelectorData<SelectorKey>
        {
            public Sprite sprite = default;
        }

        [SerializeField] private Data[] data = default;
        [SerializeField] private Image image = default;


        protected override void OnSelect(SelectorKey key, Data data)
        {
            image.sprite = data.sprite;
            image.SetNativeSize();
        }


        protected override Data FindData(SelectorKey key)
        {
            Data foundData = Array.Find(data, e => e.key.IsEquals(key));

            if (foundData == null)
            {
                CustomDebug.Log($"type={key.type}");
                CustomDebug.Log($"shouldCheckNextLeagueAchived={key.shouldCheckNextLeagueAchived}");
                CustomDebug.Log($"isNextLeagueAchived={key.isNextLeagueAchived}");
            }

            return foundData;
        }
    }
}
