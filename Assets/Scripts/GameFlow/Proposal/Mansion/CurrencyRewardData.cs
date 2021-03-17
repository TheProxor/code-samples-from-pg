using System;


namespace Drawmasters
{
    [Serializable]
    public class CurrencyRewardData 
    {
        public CurrencyType currencyType = default;
        public float value = default;

        public CurrencyRewardData(CurrencyType _currencyType, float _value)
        {
            currencyType = _currencyType;
            value = _value;
        }
    }
}
