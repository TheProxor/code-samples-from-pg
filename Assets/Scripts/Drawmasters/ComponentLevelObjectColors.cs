using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class ComponentLevelObjectColors : ComponentLevelObject
    {
        #region Nested types

        [Serializable]
        public class SerializableDataColors
        {
            public ShooterColorType colorType = default;
        }

        #endregion



        #region Fields

        private SerializableDataColors loadedDataColors;

        #endregion



        #region Properties

        public virtual ShooterColorType ColorType
        {
            get
            {
                if (!ShouldLoadColorData)
                {
                    CustomDebug.Log("Attempt to use ColorType when color data should not be loaded");
                }

                if (!IsColorsDataLoaded)
                {
                    CustomDebug.Log("Attempt to get Color Type before data is loaded. Return default");
                    return default;
                }

                return loadedDataColors.colorType;
            }
        }

        public virtual bool ShouldLoadColorData => true;

        public bool IsColorsDataLoaded => loadedDataColors != null;
        #endregion



        #region Methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            if (!ShouldLoadColorData)
            {
                return;
            }

            loadedDataColors = JsonUtility.FromJson<SerializableDataColors>(data.colorsInfo);

            if (loadedDataColors == null)
            {
                CustomDebug.Log("Cannot load color data.");
                return;
            }
        }


        public override void StartGame(GameMode mode, WeaponType weaponType, Transform levelTransform)
        {
            base.StartGame(mode, weaponType, levelTransform);

            RefreshVisualColor();
        }


        public override void FinishGame()
        {
            loadedDataColors = null;

            base.FinishGame();
        }

        protected abstract void RefreshVisualColor();


        protected override void FinishReturnToInitialState()
        {
            base.FinishReturnToInitialState();

            RefreshVisualColor();
        }

        #endregion
    }
}
