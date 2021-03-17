using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LaserLevelObject : ComponentLevelObject
    {
        #region Fields

        public event Action OnShouldDisableLaser;

        public event Action<ILaserDestroyable> OnObjectHitted;
        public event Action<Projectile> OnProjectileHitted;

        [SerializeField] private Transform fxRoot = default;

        private List<LaserComponent> components;


        #endregion



        #region Properties

        public Transform FxRoot => fxRoot;

        #endregion



        #region Methods

        protected override void InitializeComponents()
        {
            if (components == null)
            {
                components = new List<LaserComponent>
                {
                    new LaserObjectsDestroyComponent(),
                    new LaserVisualSpreadComponent()
                };
            }

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


        public void StartDestroyObject(ILaserDestroyable hittedObject) =>
            OnObjectHitted?.Invoke(hittedObject);


        public void MarkProjectileHitted(Projectile projectile) =>
            OnProjectileHitted?.Invoke(projectile);


        protected override void OnFreeFall()
        {
            base.OnFreeFall();

            OnShouldDisableLaser?.Invoke();
        }

        #endregion
    }
}
