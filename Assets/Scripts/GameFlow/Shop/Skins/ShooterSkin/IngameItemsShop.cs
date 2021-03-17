using System.Collections.Generic;
using System;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.ServiceUtil;

namespace Drawmasters
{
    /// <summary>
    /// Base class for ingame items. Not used now.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class IngameItemsShop<T> : IContentOpen
    {
        #region Fields

        public event Action<T> OnOpened;
        public event Action OnAnyContentOpened;

        private readonly string savedKey;

        private readonly IPlayerStatisticService playerStatistic;

        #endregion



        #region Properties

        public List<T> BoughtItems { get; private set; }

        protected abstract List<T> InitialBoughtSkins { get; }

        #endregion



        #region Class lifecycle

        public IngameItemsShop(string _savedKey,
                               IPlayerStatisticService _playerStatistic)
        {
            savedKey = _savedKey;
            BoughtItems = CustomPlayerPrefs.GetObjectValue<List<T>>(savedKey);
            BoughtItems = BoughtItems ?? InitialBoughtSkins;

            playerStatistic = _playerStatistic;

            foreach (var initialSkinToBuy in InitialBoughtSkins)
            {
                if (!IsBought(initialSkinToBuy))
                {
                    Open(initialSkinToBuy);
                }
            }

            SaveData();
        }

        #endregion



        #region Methods

        public void Buy(T type)
        {
            bool isAlreadyBought = IsBought(type);

            if (isAlreadyBought)
            {
                CustomDebug.Log($"Already item bought. {type} this {this}.");
            }
            else
            {
                float price = GetItemPrice(type);

                if (playerStatistic.CurrencyData.TryRemoveCurrency(CurrencyType.Simple, price))
                {
                    Open(type);
                }
            }
        }


        public void CancelBought(T type)
        {
            if (!IsBought(type))
            {
                CustomDebug.Log($"Item isn't bought. {type} this {this}.");
            }
            else
            {
                BoughtItems.Remove(type);
                SaveData();
            }
        }


        public void Open(T type)
        {
            if (!BoughtItems.Contains(type))
            {
                BoughtItems.Add(type);

                SaveData();

                OnOpened?.Invoke(type);
                OnAnyContentOpened?.Invoke();
            }
        }

        public bool IsBought(T type) => BoughtItems.Exists(info => info.Equals(type));


        protected abstract float GetItemPrice(T type);


        public void OpenAll()
        {
            foreach (var i in (T[])Enum.GetValues(typeof(T)))
            {
                if (!i.Equals((T)default))
                {
                    Open(i);
                }
            }
        }


        private void SaveData() => CustomPlayerPrefs.SetObjectValue(savedKey, BoughtItems);

        #endregion
    }
}
