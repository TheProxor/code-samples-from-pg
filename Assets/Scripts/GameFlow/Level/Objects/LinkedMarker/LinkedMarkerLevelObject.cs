using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LinkedMarkerLevelObject : ComponentLevelObject
    {

        #region Fields

        [SerializeField] private Animator mainAnimator = default;

        private List<LevelObjectComponentTemplate<LinkedMarkerLevelObject>> components;

        #endregion



        #region Properties

        public Animator MainAnimator =>
            mainAnimator;

        #endregion



        #region ComponentLevelObject

        protected override void InitializeComponents()
        {
            components = components ?? new List<LevelObjectComponentTemplate<LinkedMarkerLevelObject>>
            {
                new LinkedMarkerShowHideComponent(),
                new LinkedMarkerPhysicalLevelObjectMonitorComponent(),
                new LinkedMarkerLevelTargetMonitorComponent()
            };

            foreach (var component in components)
            {
                component.Initialize(this);
            }
        }


        protected override void EnableComponents()
        {
            foreach (var component in components)
            {
                component.Enable();
            }
        }


        protected override void DisableComponents()
        {
            foreach (var component in components)
            {
                component.Disable();
            }
        }

        #endregion
    }
}
