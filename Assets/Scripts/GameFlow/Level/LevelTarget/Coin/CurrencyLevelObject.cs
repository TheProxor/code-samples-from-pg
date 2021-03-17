using System;
using System.Collections.Generic;
using UnityEngine;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Levels
{
    public class CurrencyLevelObject : ComponentLevelObject
    {
        #region Nested types

        [Serializable]
        public class SerializableData
        {
            public CurrencyType CurrencyType = CurrencyType.Simple;
            public float CurrencyCount = 1;
        }

        #endregion



        #region Fields

        [SerializeField] private CollisionNotifier collisionNotifier = default;
        [SerializeField] private Collider2D mainCollider2D = default;
        [SerializeField] private MeshRenderer mainRenderer = default;
        [SerializeField] private MeshFilter mainMeshFilter = default;

        private List<LevelObjectComponentTemplate<CurrencyLevelObject>> components;

        #endregion



        #region Properties

        public Collider2D MainCollider2D => mainCollider2D;

        public CurrencyType CurrencyType { get; private set; } = CurrencyType.Simple;
        
        public float CurrencyCount { get; private set; } = 1;

        #endregion




        #region Methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            SerializableData additionalData = JsonUtility.FromJson<SerializableData>(data.additionalInfo);

            if (additionalData != null)
            {
                CurrencyType dataParsedCurrencyType = additionalData.CurrencyType;

                CurrencyType = dataParsedCurrencyType.IsAvailableForShow() ?
                    dataParsedCurrencyType : 
                    IngameData.Settings.coinLevelObjectSettings.mansionReplacedCurrencyType;

                CurrencyCount = additionalData.CurrencyCount;
            }
        }

        protected override void InitializeComponents()
        {
            if (components == null)
            {
                components = new List<LevelObjectComponentTemplate<CurrencyLevelObject>>
                {
                    new CoinCollectComponent(collisionNotifier),
                    new CoinVisualComponent(mainRenderer, mainMeshFilter),
                    new CoinMoveComponent()
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