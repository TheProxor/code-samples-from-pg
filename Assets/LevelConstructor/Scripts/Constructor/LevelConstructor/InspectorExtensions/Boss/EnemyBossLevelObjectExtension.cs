using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EnemyBossLevelObjectExtension : InspectorExtensionColorsBase
    {
        #region Fields

        [SerializeField] private SliderInputUi spriteChangeSlider = default;

        [Header("stages")]
        [SerializeField] private EnemyBossExtensionRocketDraw enemyBossExtensionRocketDraw = default;
        [SerializeField] private EnemyBossExtensionMove enemyBossExtensionMove = default;

        private EditorEnemyBossLevelObject enemyBossLevelObject;

        private string[] availableSkins;

        #endregion



        #region Methods

        public override void Init(EditorLevelObject levelObject)
        {
            base.Init(levelObject);

            availableSkins = IngameData.Settings.bossLevelTargetSettings.skins;

            enemyBossLevelObject = levelObject as EditorEnemyBossLevelObject;

            if (enemyBossLevelObject != null)
            {
                spriteChangeSlider.MarkWholeNumbersOnly();
                spriteChangeSlider.Init("Skin", enemyBossLevelObject.SkinIndex, 0.0f, availableSkins.Length - 1.0f);

                RefreshObjectSkin();
            }

            enemyBossExtensionRocketDraw.Init(enemyBossLevelObject);
            enemyBossExtensionMove.Init(enemyBossLevelObject);
        }


        protected override void SubscribeOnEvents()
        {
            base.SubscribeOnEvents();

            spriteChangeSlider.OnValueChange += SpriteChangeSlider_OnValueChange;
            enemyBossExtensionRocketDraw.SubscribeOnEvents();
            enemyBossExtensionMove.SubscribeOnEvents();
        }


        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();

            spriteChangeSlider.OnValueChange -= SpriteChangeSlider_OnValueChange;
            enemyBossExtensionRocketDraw.UnsubscribeFromEvents();
            enemyBossExtensionMove.UnsubscribeFromEvents();
        }


        private void RefreshObjectSkin() =>
            enemyBossLevelObject.SetSkin(availableSkins[enemyBossLevelObject.SkinIndex]);

        #endregion



        #region Events handlers

        private void SpriteChangeSlider_OnValueChange(float value)
        {
            int spriteIndex = Mathf.CeilToInt(value);

            enemyBossLevelObject.SkinIndex = spriteIndex;
            enemyBossLevelObject.RefreshSerializableData();
            RefreshObjectSkin();
        }

        #endregion
    }
}