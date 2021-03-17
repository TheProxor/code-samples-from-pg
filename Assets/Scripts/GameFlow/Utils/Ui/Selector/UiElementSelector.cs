using System;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiElementSelectorData<K>
    {
        public K key = default;
    };


    public abstract class UiElementSelector<T, K> where T : UiElementSelectorData<K>
    {
        public void Select(K key)
        {
            T data = FindData(key);

            if (data == null)
            {
                CustomDebug.Log($"No data found in {this} for {key}");
                return;
            }

            OnSelect(key, data);
        }

        protected abstract void OnSelect(K key, T data);

        protected abstract T FindData(K key);
    }
}
