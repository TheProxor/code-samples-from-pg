using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class CoinCollectComponent : LevelObjectComponentTemplate<CurrencyLevelObject>
    {
        #region Fields

        public static event Action<CurrencyLevelObject, ICoinCollector> OnShouldCollectCoin;

        private readonly CollisionNotifier collisionNotifier;

        #endregion



        #region Class lifecycle

        public CoinCollectComponent(CollisionNotifier _collisionNotifier)
        {
            collisionNotifier = _collisionNotifier;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            collisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
        }


        public override void Disable()
        {
            collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D collision)
        {
            CollidableObject collidableObject = collision.gameObject.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            ICoinCollector collector = collidableObject.CoinCollector;
            if (collector != null)
            {
                collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;

                OnShouldCollectCoin?.Invoke(levelObject, collector);
            }
        }

        #endregion
    }
}
