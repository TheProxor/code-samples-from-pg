using System;
using UnityEngine;


namespace Drawmasters.Levels.Order
{
    [Serializable]
    public class ColorProfile
    {
        [Header("Monolith fill texture color")]
        public Color monolithFillColor = default;

        [Header("Monolith edge texture color")]
        public Color monolithEdgeColor = default;

        [Header("Background group type")]
        public int backgroundIndex = default;

        [Header("Monolith fill texture weapon type")]
        public MonolithFillType monolithFillType = default;
    }
}