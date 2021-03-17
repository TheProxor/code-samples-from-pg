using UnityEngine;
using Spine;
using Spine.Unity;

namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "HappyHoursVisualSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "HappyHoursVisualSettings")]
    public class HappyHoursVisualSettings : ScriptableObject
    {
        #region Fields

        public NumberAnimation numberAnimation = default;
        public VectorAnimation bounceInAnimation = default;
        public VectorAnimation bounceOutAnimation = default;
        public ColorAnimation colorAnimation = default;

        public float soundDelay = default;

        #endregion
    }
}
