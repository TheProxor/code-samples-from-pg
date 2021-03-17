using System;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class CurrencyObjectsLevelController : ILevelController
    {
        #region Fields

        public event Action<CurrencyLevelObject, ICurrencyAbsorber> OnShouldAbsorbObject;

        private readonly List<CurrencyLevelObject> objects = new List<CurrencyLevelObject>();

        #endregion



        #region Public methods

        public void Initialize()
        {
            CoinCollectComponent.OnShouldCollectCoin += CoinCollectComponent_OnShouldCollectCoin;
        }


        public void Deinitialize()
        {
            CoinCollectComponent.OnShouldCollectCoin -= CoinCollectComponent_OnShouldCollectCoin;

            for (int i = objects.Count - 1; i > -1; i--)
            {
                Remove(objects[i]);
            }
        }


        public void AbsorbAllCurrencyObjects(ICurrencyAbsorber currencyAbsorber)
        {
            if (currencyAbsorber == null)
            {
                CustomDebug.Log($"Can't perfom {System.Reflection.MethodBase.GetCurrentMethod().Name} " +
                                $"as arg {nameof(ICurrencyAbsorber)} is null");
                return;
            }

            foreach (var obj in objects)
            {
                OnShouldAbsorbObject?.Invoke(obj, currencyAbsorber);
            }
        }


        public void Add(CurrencyLevelObject target) =>
            objects.Add(target);
        

        private void Remove(CurrencyLevelObject target) =>
            objects.Remove(target);
        
        #endregion



        #region Events handlers

        private void CoinCollectComponent_OnShouldCollectCoin(CurrencyLevelObject anotherCurrencyObject, ICoinCollector coinCollector)
        {
            Remove(anotherCurrencyObject);
        }

        #endregion
    }
}

