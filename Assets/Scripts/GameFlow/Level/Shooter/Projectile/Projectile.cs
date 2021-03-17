using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class Projectile : SerializedMonoBehaviour, IDeinitializable, ICoinCollector, ILaserDestroyable
    {
        #region Fields

        public event Action<bool> OnShouldLaserDestroy;
        public event Action<CollidableObject> OnShouldSmash;
        public event Action OnShouldStop;

        public event Action<Vector2[]> OnShouldShot; // trajcetory path

        public event Action<Projectile> OnShouldDestroy;

        [SerializeField] protected Rigidbody2D mainRigidbody2D = default;

        [SerializeField] protected List<CollidableObjectType> typesThatDestroyProjectile = default;
        [SerializeField] protected List<PhysicalLevelObjectType> physicalLevelObjectTypes = default;

        [SerializeField] protected CollisionNotifier projectileCollisionNotifier = default;

        [SerializeField] protected SpriteRenderer mainRenderer = default;
        [SerializeField] private Transform root = default;

        private List<ProjectileComponent> projectileComponents;

        #endregion



        #region Properties

        public Transform Root => root;

        public Transform ProjectileSpriteRoot => mainRenderer.transform;

        public PreviousFrameRigidbody2D PreviousFrameRigidbody2D { get; private set; }

        public Rigidbody2D MainRigidbody2D => mainRigidbody2D;

        public ShooterColorType  ColorType { get; private set; }

        public CollisionNotifier ProjectileCollisionNotifier => projectileCollisionNotifier;

        public abstract ProjectileType Type { get; }

        protected virtual List<ProjectileComponent> CoreComponents => new List<ProjectileComponent>() { };

        #endregion



        #region Methods

        public void SetupColorType(ShooterColorType colorType) =>
             ColorType = colorType;

        public void InvokeShotEvent(Vector2[] trajectory)
        {
            PreviousFrameRigidbody2D.Initialize();

            OnShouldShot?.Invoke(trajectory);
        }

        public void StopTrajectoryPath() =>
            OnShouldStop?.Invoke();


        public void Smash(CollidableObject collidableObject) =>
            OnShouldSmash?.Invoke(collidableObject);


        public void Destroy() => OnShouldDestroy?.Invoke(this);
       

        public void Initialize(WeaponType mode)
        {
            projectileComponents = CoreComponents;

            InitializeComponents(mode);
        }


        public void Deinitialize()
        {
            PreviousFrameRigidbody2D.Deinitialize();

            DeinitializeComponents();
        }


        private void InitializeComponents(WeaponType mode)
        {
            foreach (var component in projectileComponents)
            {
                component.Initialize(this, mode);
            }

            PreviousFrameRigidbody2D = new PreviousFrameRigidbody2D(mainRigidbody2D);
        }


        private void DeinitializeComponents()
        {
            foreach (var component in projectileComponents)
            {
                component.Deinitialize();
            }
        }

        public void StartImmediatelyLaserDestroy() =>
            OnShouldLaserDestroy?.Invoke(true);

        public void StartLaserDestroy() =>
            OnShouldLaserDestroy?.Invoke(false);

        #endregion



        #region ICoinCollector

        public Vector2 CurrentPosition =>
            MainRigidbody2D.position;

        #endregion
    }
}
