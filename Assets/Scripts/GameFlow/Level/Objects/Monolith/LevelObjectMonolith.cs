using MiniJSON;
using Drawmasters.Levels.Order;
using Drawmasters.Monolith;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.Levels.Data;

namespace Drawmasters.Levels
{
    public partial class LevelObjectMonolith : LevelObject
    {
        #region Nested Types

        public class SerializableData
        {
            public List<PointData> monolithPointsData = default;
            public bool isOpedEnded = default;


            public SerializableData()
            {
                monolithPointsData = new List<PointData>();
            }
        }

        #endregion



        #region Fields

        [SerializeField] private SpriteShapeController shapeController = default;

        private List<CornerGraphic> cornerObjects = new List<CornerGraphic>();

        private SerializableData serializableData;

        private WeaponType currentWeaponType;

        private ILevelEnvironment levelEnvironment;

        #endregion



        #region Properties

        public Spline Spline => shapeController.spline;

        public Collider2D MonolithCollider => shapeController.edgeCollider;
        
        private Material EdgeSharedMaterial => shapeController.spriteShapeRenderer.sharedMaterials[1];

        private Material FillSharedMaterial => shapeController.spriteShapeRenderer.sharedMaterials[0];


        #endregion



        #region Methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            serializableData = Json.Deserialize<SerializableData>(data.additionalInfo);

            shapeController.spline.isOpenEnded = serializableData.isOpedEnded;
            shapeController.spline.Clear();
        }


        public override void StartGame(GameMode mode, WeaponType weaponType, Transform levelTransform)
        {
            base.StartGame(mode, weaponType, levelTransform);

            currentWeaponType = (UaWeaponTypeMonolith == WeaponType.None) ? weaponType : UaWeaponTypeMonolith;
            levelEnvironment = GameServices.Instance.LevelEnvironment;

            SpriteShape spriteShape = IngameData.Settings.monolith.CreateSpriteShape();

            LevelContext context = levelEnvironment.Context;

            ColorProfile colorProfile = IngameData.Settings.colorProfilesSettings.GetProfile(context.ColorProfileIndex);

            if (shapeController != null)
            {
                shapeController.spriteShape = spriteShape;
            }

            RenderMonolith();

            SubscribeOnUaEvents();

            if (!UseColoredFillTexture)
            {
                RepaintFillTexture(colorProfile.monolithFillColor, colorProfile.monolithFillType);
            }

            if (!UseColoredEdgeTexture)
            {
                RepaintEdgeTexture(colorProfile.monolithEdgeColor);
            }
        }


        public override void FinishGame()
        {
            UnsubscribeFromUaEvents();

            cornerObjects.ForEach(corner => Content.Management.DestroyCorner(corner));
            cornerObjects.Clear();

            base.FinishGame();
        }


        public override void StartMoving()
        {
            if (CanMove)
            {
                // hot fix for unity 2019 and DoTween
                Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            base.StartMoving();
        }



        private void RepaintFillTexture(Color fillColor, MonolithFillType fillType)
        {
            Texture2D sourceColorableTexture = IngameData.Settings.monolith.GetFillTexture(fillType);

            shapeController.spriteShape.fillTexture = sourceColorableTexture;
            FillSharedMaterial.color = fillColor;

            shapeController.RefreshSpriteShape();
        }


        private void RepaintEdgeTexture(Color edgeColor)
        {
            Sprite sprite = IngameData.Settings.monolith.GetMonolithConturSprite();

            shapeController.spriteShape.angleRanges.First().sprites[0] = sprite;
            EdgeSharedMaterial.color = edgeColor;
            
            shapeController.RefreshSpriteShape();
        }


        #endregion



        #region Events handlers

        private void RenderMonolith()
        {
            MonolithRenderUtility.RenderMonolith(serializableData.monolithPointsData,
                                                 ref cornerObjects,
                                                 shapeController.edgeCollider,
                                                 shapeController.spline,
                                                 transform,
                                                 currentWeaponType,
                                                 true);
        }

        #endregion
    }
}
