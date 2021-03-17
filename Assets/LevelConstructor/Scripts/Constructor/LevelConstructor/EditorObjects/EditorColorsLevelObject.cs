using Drawmasters.Levels;
using UnityEngine;



namespace Drawmasters.LevelConstructor
{
    public abstract class EditorColorsLevelObject : EditorLevelObject
    {
        #region Fields

        private ComponentLevelObjectColors.SerializableDataColors serializableDataColors = default;

        #endregion



        #region Properties

        public ShooterColorType ColorType { get; set; }

        #endregion



        #region Methods

        public override LevelObjectData GetData()
        {
            LevelObjectData data = base.GetData();

            data.colorsInfo = JsonUtility.ToJson(serializableDataColors);

            return data;
        }


        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            if (!string.IsNullOrEmpty(data.colorsInfo))
            {
                var loadedData = JsonUtility.FromJson<ComponentLevelObjectColors.SerializableDataColors>(data.colorsInfo);

                ColorType = loadedData.colorType;

                if (ColorType == ShooterColorType.None)
                {
                    ColorType = ShooterColorType.Red;
                    LoadDefaultData();
                }
            }

            Refresh();
        }


        public void Refresh()
        {
            if (serializableDataColors != null)
            {
                serializableDataColors.colorType = ColorType;
            }

            OnColorTypeRefresh(ColorType);
        }

        public virtual void OnColorTypeRefresh(ShooterColorType shooterColorType) { }


        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            if (serializableDataColors == null)
            {
                serializableDataColors = new ComponentLevelObjectColors.SerializableDataColors
                {
                    colorType = ColorType
                };
            }
        }

        #endregion
    }
}
