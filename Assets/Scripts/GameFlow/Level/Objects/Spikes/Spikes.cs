using System;
using System.Collections.Generic;
using Drawmasters.Effects;
using Drawmasters.Effects.Helpers.MonitorHelper.Enum;
using Drawmasters.Levels.Data;
using Drawmasters.Levels.Order;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class Spikes : PhysicalLevelObject
    {
        #region Fields

        public const float TileWidth = 13.4f;

        public event Action<PhysicalLevelObject> OnShouldConnectObject;

        #endregion



        #region Properties

        public Color? Color { get; private set; }

        #endregion



        #region Methods

        public void PlayFx(string fxKey)
        {
            const float TileWidth = 13.4f;
            int tilesCount = Mathf.RoundToInt(SpriteRenderer.size.x / TileWidth);

       //     SpriteRenderer spriteRendererDecal = SpriteRenderer;

            for (int i = 0; i < tilesCount; i++)
            {
                Vector3 fxPos = SpriteRenderer.transform.localPosition;
                fxPos = new Vector3((-SpriteRenderer.size.x + TileWidth) * 0.5f + i * TileWidth, 0.0f, 0.0f);

                EffectHandler handler = EffectManager.Instance.CreateSystem(fxKey,
                    true,
                    fxPos,
                    SpriteRenderer.transform.rotation,
                    SpriteRenderer.transform,
                    TransformMode.Local,
                    shouldOverrideLoops: false,
                    MonitorEffectType.LevelUnload);
                if (handler != null)
                {
                    handler.Play();
                }
            }
        }
        
        
        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            if (PhysicalData.shapeType == PhysicalLevelObjectShapeType.Spikes &&
                PhysicalData.type == PhysicalLevelObjectType.Monolit)
            {
                ChangeSpriteColor();
            }

            spriteRenderer.size = new Vector2(LoadedData.width, spriteRenderer.size.y);
        }


        public override void SetLinks(List<LevelObject> linkedObjects)
        {
            base.SetLinks(linkedObjects);

            if (!IsLinkedObjectsPart)
            {
                return;
            }

            foreach (var objectToLink in linkedObjects)
            {
                if (objectToLink is PhysicalLevelObject physicalLevelObject)
                {
                    OnShouldConnectObject?.Invoke(physicalLevelObject);
                    break;
                }
                else
                {
                    CustomDebug.Log("Not implemented logic for links");
                }
            }
        }


        private void ChangeSpriteColor()
        {
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            ColorProfile colorProfile = IngameData.Settings.colorProfilesSettings.GetProfile(context.ColorProfileIndex);
            Color = colorProfile.monolithEdgeColor;

            SpriteRenderer.color = Color.Value;
        }

        #endregion
    }
}
