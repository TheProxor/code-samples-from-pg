using Drawmasters.Effects;
using Drawmasters.Ui;
using Drawmasters.ServiceUtil;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace Drawmasters.Levels
{
    public class BonusLevelVfxHelper : IInitializable, IDeinitializable
    {
        #region Fields

        private readonly BonusLevelController bonusLevelController;
        private readonly IngameScreen screen;
        private readonly HashSet<int> indexesStagesWithVfx = new HashSet<int>();
        private readonly HashSet<GameObject> fxSprites = new HashSet<GameObject>();

        #endregion



        #region Ctor

        public BonusLevelVfxHelper(BonusLevelController _bonusLevelController, IngameScreen _screen)
        {
            bonusLevelController = _bonusLevelController;
            screen = _screen;
        }
        
        #endregion
        
        
        
        #region IInitializable
        
        public void Initialize()
        {
            bonusLevelController.OnDecelerationBegin += BonusLevelController_OnDecelerationBegin;
            bonusLevelController.OnStopObjects += BonusLevelController_OnStopObjects;
            bonusLevelController.OnStageBegun += BonusLevelController_OnStageBegun;

            indexesStagesWithVfx.Clear();
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            ClearFrozenSprites();

            bonusLevelController.OnDecelerationBegin -= BonusLevelController_OnDecelerationBegin;
            bonusLevelController.OnStopObjects -= BonusLevelController_OnStopObjects;
            bonusLevelController.OnStageBegun -= BonusLevelController_OnStageBegun;

            DOTween.Kill(this);
        }

        #endregion



        #region Methods

        private void ClearFrozenSprites()
        {
            foreach (var sprite in fxSprites)
            {
                if (sprite != null)
                {
                    Object.Destroy(sprite);
                }
            }

            fxSprites.Clear();
        }

        #endregion



        #region Events handlers

        private void BonusLevelController_OnStopObjects(int stageIndex)
        {
            if (!indexesStagesWithVfx.Contains(stageIndex))
            {
                indexesStagesWithVfx.Add(stageIndex);

                Camera uiCamera = UiCamera.Instance.Camera;

                Vector3 upperRight = uiCamera.ViewportToWorldPoint(new Vector3(1f, 1f));
                EffectHandler handler = EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIBonusLevelFrozenEdgeRightUp, upperRight, parent: uiCamera.transform);
                handler.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));

                Vector3 upperLeft = uiCamera.ViewportToWorldPoint(new Vector3(0f, 1f));
                handler = EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIBonusLevelFrozenEdgeLeftUp, upperLeft, parent: uiCamera.transform);
                handler.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));

                Vector3 lowerRight = uiCamera.ViewportToWorldPoint(new Vector3(1f, 0f));
                handler = EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIBonusLevelFrozenEdgeRightDown, lowerRight, parent: uiCamera.transform);
                handler.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));

                Vector3 lowerLeft = uiCamera.ViewportToWorldPoint(new Vector3(0f, 0f));
                handler = EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIBonusLevelFrozenEdgeLeftDown, lowerLeft, parent: uiCamera.transform);
                handler.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));

                List<PhysicalLevelObject> allPhysicalObjects = GameServices.Instance.LevelControllerService.PhysicalObjects.AllObjects;
                BonusLevelSettings settings = IngameData.Settings.bonusLevelSettings;

                foreach (var physicalObject in allPhysicalObjects)
                {
                    string fxKey = settings.FindFrezeeFx(physicalObject.PhysicalData);
                    EffectManager.Instance.PlaySystemOnce(fxKey, 
                        physicalObject.transform.position, 
                        physicalObject.transform.rotation, 
                        physicalObject.transform);

                    CurrencyType type = physicalObject.CurrentData.bonusData.currencyType;
                    
                    (Sprite, Vector3) spriteAndScale = settings.FindFrezeeSpriteAndScale(physicalObject.PhysicalData, physicalObject.CurrentData.bonusData);

                    GameObject go = new GameObject($"{physicalObject.name}FrozenSprite");
                    go.transform.position = physicalObject.transform.position;
                    go.transform.rotation = physicalObject.transform.rotation;
                    go.transform.SetParent(physicalObject.transform);

                    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                    renderer.sprite = spriteAndScale.Item1;
                    renderer.sortingLayerName = RenderLayers.LevelObject;
                    renderer.sortingOrder = physicalObject.SpriteRenderer.sortingOrder + 1;

                    settings.spriteScaleAnimation.SetupEndValue(spriteAndScale.Item2);
                    settings.spriteScaleAnimation.Play((value) => go.transform.localScale = value, this);

                    fxSprites.Add(go);
                }
            }
        }

        
        private void BonusLevelController_OnStageBegun(int stage) =>
            ClearFrozenSprites();
        

        private void BonusLevelController_OnDecelerationBegin(int stageIndex)
        {
            var i = EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIBonusLevelSlowMo, screen.SlowmoVfxPosition, parent: null);
            i.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
        }
        
        #endregion

    }
}
