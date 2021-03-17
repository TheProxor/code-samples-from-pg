using System;
using UnityEngine;
using Drawmasters.Ua;


namespace Drawmasters.Levels
{
	public class VisualLevelObject : LevelObject
	{
		#region Nested types

		[Serializable]
		public class SerializableData
		{
            public int spriteIndex = default;
            public int sortingOrder = default;
		}

        #endregion



        #region Fields

        [SerializeField] private SpriteRenderer spriteRenderer = default; 

        private SerializableData loadedData;

        private int spriteIndex;

        #endregion



        #region Properties

        public static bool ShouldShowMonolithDecalsUa { get; set; }
        public static bool ShouldShowLevelDecalsUa { get; set; }

        #endregion



        #region Methods

        public override void SetData(LevelObjectData data)
		{
			base.SetData(data);

            loadedData = JsonUtility.FromJson<SerializableData>(data.additionalInfo);
            spriteIndex = loadedData.spriteIndex;
        }


        protected override void FinishReturnToInitialState()
        {
            base.FinishReturnToInitialState();

            if (loadedData != null)
            {
                InitializeSprite(WeaponType);
                spriteRenderer.sortingOrder = loadedData.sortingOrder;
            }
        }

        public override void StartGame(GameMode mode, WeaponType weaponType, Transform levelTransform)
        {
            base.StartGame(mode, weaponType, levelTransform);

            RefreshVisualUa();

            PuzzlemastersDevContent.OnDecalsShowChanged += RefreshVisualUa;
        }


        public override void FinishGame()
        {
            PuzzlemastersDevContent.OnDecalsShowChanged -= RefreshVisualUa;

            base.FinishGame();
        }


        private void InitializeSprite(WeaponType weaponType)
        {
            CommonVisualObjectsSettings settings = IngameData.Settings.commonVisualObjectsSettings;

            Sprite spriteToSet = settings.FindSprite(weaponType, loadedData.spriteIndex);

            if (spriteToSet != null)
            {
                spriteRenderer.sprite = spriteToSet;
            }
        }


        private void RefreshVisualUa()
        {
            bool isUaGroup = IngameData.Settings.commonVisualObjectsSettings.IsUaGroupSprite(WeaponType, spriteIndex);
            spriteRenderer.enabled = isUaGroup ? ShouldShowMonolithDecalsUa : ShouldShowLevelDecalsUa;
        }

        #endregion
    }
}
