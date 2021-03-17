using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class EnemyBossGeneric<T> : EnemyBossBase where T : BossSerializableData
    {
        #region Fields

        protected T loadedData;

        #endregion



        #region Overrided methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            loadedData = JsonUtility.FromJson<T>(data.additionalInfo);
            if (loadedData != null)
            {
                LoadAdditionalData(loadedData);
            }
            else
            {
                CustomDebug.Log($"Cannot parse data for boss. Raw string : {data.additionalInfo}");
            }
        }

        #endregion



        #region Protected methods

        protected virtual void LoadAdditionalData(T data)
        {
            ApplySkin(data.skinIndex);
        }

        #endregion
    }
}
