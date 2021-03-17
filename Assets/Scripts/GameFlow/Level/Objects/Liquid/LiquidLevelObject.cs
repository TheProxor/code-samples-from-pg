using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.U2D;
using UnityEngine.U2D;


namespace Drawmasters.Levels
{
    public class LiquidLevelObject : ComponentLevelObject
    {
        #region Nested types

        [Serializable]
        public class SerializableData
        {
            public Vector2 size = default;
            public LiquidLevelObjectType type = default;
        }

        #endregion



        #region Fields

        [SerializeField] private SpriteRenderer gradientSpriteRenderer = default;
        [SerializeField] private BoxCollider2D mainCollider2D = default;
        [SerializeField] private CollisionNotifier collisionNotifier = default;
        [SerializeField] private SpriteShapeController spriteShapeController = default;

        private List<LiquidComponent> components;

        #endregion



        #region Properties

        public CollisionNotifier CollisionNotifier => collisionNotifier;


        public BoxCollider2D MainCollider2D => mainCollider2D;


        public SpriteShapeRenderer SpriteShapeRenderer => spriteShapeController.spriteShapeRenderer;


        public Spline Spline { get; private set; }


        public SerializableData LoadedData { get; private set; }

        #endregion



        #region Methods

        public override void SetData(LevelObjectData data)
        {
            SerializableData CurrentData = JsonUtility.FromJson<SerializableData>(data.additionalInfo);

            LoadedData = new SerializableData
            {
                size = CurrentData.size,
                type = CurrentData.type
            };

            Spline = spriteShapeController.spline;
            MainCollider2D.size = LoadedData.size;
            gradientSpriteRenderer.size = LoadedData.size;

            base.SetData(data);
        }


        protected override void InitializeComponents()
        {
            if (components == null)
            {
                components = new List<LiquidComponent>
                {
                    new LiquidSlowdownComponent(),
                    new LiquidObjectsFullyCoveredComponent(),
                    new LiquidShapeGraphicsComponent(),
                    //temporary unused
                    //new LiquidProjectileEffectComponent(),
                    new LiquidBackgroundIdleComponent()
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

        #endregion
    }
}
