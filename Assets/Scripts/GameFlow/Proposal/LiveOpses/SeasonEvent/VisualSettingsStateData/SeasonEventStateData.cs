using System;
using Drawmasters.Announcer;
using UnityEngine;


namespace Drawmasters.Proposal
{
    /// <summary>
    /// Helpers for conviniet setup in editor
    /// </summary>
    public class SeasonEventStateData
    {
        [Serializable]
        public class StateUiElementData
        {
            public UiSeasonEventRewardElement.State state = default;
            public Color textOutlineColor = Color.white;
            public Sprite back = default;

            public Sprite forcemeterRewardSprite = default;
            public Sprite spinRouletteCashRewardSprite = default;
            public Sprite spinRouletteSkinRewardSprite = default;
            public Sprite spinRouletteWaiponRewardSprite = default;

            [Header("Additional elements")]
            public string plankText = default;
        }

        [Serializable]
        public class LevelElement
        {
            public int minIndexForSprite = default;
            public Color outlineColor = default;
            public Sprite sprite = default;
        }


        [Serializable]
        public class UiElement
        {
            public SeasonEventRewardType eventRewardtype = default;
            public StateUiElementData[] stateSpriteData = default;
            public Sprite announcerBackgroundSprite = default;
            public CommonAnnouncer.Data announcerData = default;
            public Vector3 canNotClaimAnnouncerMoveOffset = default;
        }


        [Serializable]
        public class StateSpriteData
        {
            public UiSeasonEventRewardElement.State[] states = default;
            public Sprite sprite = default;
        }


        [Serializable]
        public class Currency
        {
            public SeasonEventRewardType[] eventRewardtypes = default;

            public CurrencyType type = default;
            public StateSpriteData[] stateSpriteData = default;
        }


        [Serializable]
        public class PetSkin
        {
            public SeasonEventRewardType eventRewardtype = default;

            public PetSkinType type = default;
            public StateSpriteData[] stateSpriteData = default;
        }
    }
}
