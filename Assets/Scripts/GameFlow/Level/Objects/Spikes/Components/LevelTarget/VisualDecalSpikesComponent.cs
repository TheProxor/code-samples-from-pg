using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;
using Object = UnityEngine.Object;


namespace Drawmasters.Levels
{
    public class VisualDecalSpikesComponent : LevelTargetCollisionSpikesComponent
    {
        #region Fields

        private CommonSpikesSettings settings;
        private Dictionary<int, SpriteRenderer> createdSpriteRenderers;

        #endregion



        #region Methods

        public override void Initialize(CollisionNotifier notifier, Rigidbody2D rigidbody, PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);

            settings = IngameData.Settings.commonSpikesSettings;
        }


        public override void Enable()
        {
            base.Enable();

            createdSpriteRenderers = new Dictionary<int, SpriteRenderer>();
        }


        public override void Disable()
        {
            foreach (var decal in createdSpriteRenderers)
            {
                Object.Destroy(decal.Value.gameObject);
            }

            createdSpriteRenderers.Clear();

            DOTween.Kill(this);

            base.Disable();
        }


        protected override void OnLevelLimbTargetCollision(LevelTarget levelTarget, LevelTargetLimbPart limbPart)
        {
            base.OnLevelLimbTargetCollision(levelTarget, limbPart);

            Rigidbody2D limbPartRB = levelTarget.Ragdoll2D.GetRigidbody(limbPart.BoneName);

            if (limbPartRB == null)
            {
                return;
            }

            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
            if (!playerData.IsBloodEnabled)
            {
                return;
            }

            Vector3 position = sourceLevelObject.transform.InverseTransformPoint(limbPartRB.position);

            float parameter = Mathf.InverseLerp(-sourceLevelObject.SpriteRenderer.size.x * 0.5f, sourceLevelObject.SpriteRenderer.size.x * 0.5f, position.x);

            int tilesCount = Mathf.RoundToInt(sourceLevelObject.SpriteRenderer.size.x / Spikes.TileWidth);

            int tileOrderToApplyDecal = (int)(parameter * tilesCount);
            tileOrderToApplyDecal = Mathf.Min(tileOrderToApplyDecal, tilesCount - 1);

            if (createdSpriteRenderers.ContainsKey(tileOrderToApplyDecal))
            {
                return;
            }

            SpriteRenderer spriteRendererDecal = Object.Instantiate(settings.decalSpriteRenderer, sourceLevelObject.transform);
            spriteRendererDecal.transform.position = sourceLevelObject.transform.position;
            spriteRendererDecal.transform.localPosition = spriteRendererDecal.transform.localPosition.SetX((-sourceLevelObject.SpriteRenderer.size.x + Spikes.TileWidth) * 0.5f + tileOrderToApplyDecal * Spikes.TileWidth);

            Sprite decalSprite = FindDecalSprite(tileOrderToApplyDecal, tilesCount - 1);
            spriteRendererDecal.sprite = decalSprite;

            createdSpriteRenderers.Add(tileOrderToApplyDecal, spriteRendererDecal);

            settings.alphaDecalAnimation.Play((alpha) => spriteRendererDecal.color = spriteRendererDecal.color.SetA(alpha), this);
        }


        private Sprite FindDecalSprite(int tileOrderToApplyDecal, int totalTiles)
        {
            Sprite result = default;

            int spikesSpriteIndex = sourceLevelObject.LoadedData.spriteIndex;
            CommonSpikesSettings.Data visualData = Array.Find(settings.visualData, element => element.data.Equals(sourceLevelObject.PhysicalData) &&
                                                                                              (element.spriteIndex == spikesSpriteIndex));

            if (visualData == null)
            {
                CustomDebug.Log($"No visal data for spikes for object with index {spikesSpriteIndex}");
                return null;
            }

#warning bad logic on sprite index. need refacctor vladislav.k
            if (tileOrderToApplyDecal == 0)
            {
                result = visualData.leftBorderDecalSprite;
            }
            else if (tileOrderToApplyDecal == totalTiles)
            {
                result = visualData.rightBorderDecalSprite;
            }
            else
            {
                result = visualData.middleDecalSprite;
            }

            return result;
        }

        #endregion
    }
}
