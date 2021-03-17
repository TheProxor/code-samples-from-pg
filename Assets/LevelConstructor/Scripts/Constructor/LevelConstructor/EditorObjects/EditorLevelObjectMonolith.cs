using MiniJSON;
using Drawmasters.Levels;
using Drawmasters.Monolith;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;


namespace Drawmasters.LevelConstructor
{
    public class EditorLevelObjectMonolith : EditorLevelObject, IUpdatablePointStructure
    {
        #region Fields

        private const ShapeTangentMode defaultShapeTangentMode = ShapeTangentMode.Linear;

        [SerializeField] private SpriteShapeController shapeController = default;

        private LevelObjectMonolith.SerializableData serializableData;

        private List<CornerGraphic> cornerObjects = new List<CornerGraphic>();

        #endregion



        #region Properties

        public bool IsOpenEnded
        {
            get
            {
                bool result = false;

                if (serializableData != null)
                {
                    result = serializableData.isOpedEnded;
                }

                return result;
            }
        }


        public override Vector3 Center
        {
            get
            {
                PointHandler[] points = GetComponentsInChildren<PointHandler>();
                return CommonUtility.CalculateCentralPoint(points);
            }
        }

        #endregion



        #region Methods

        public override LevelObjectData GetData()
        {
            LevelObjectData data = base.GetData();

            data.additionalInfo = Json.Serialize(serializableData);

            RefreshSpriteShape();
            return data;
        }


        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            serializableData = Json.Deserialize<LevelObjectMonolith.SerializableData>(data.additionalInfo);

            MonolithRenderUtility.RenderMonolith(PointData,
                                                 ref cornerObjects,
                                                 shapeController.polygonCollider,
                                                 shapeController.spline,
                                                 transform,
                                                 WeaponType.Sniper);

            RefreshSpriteShape();
        }


        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("MonolithLevelObjectInspectorExtension.prefab");


        public void SetOpenEndedState(bool isOpenEnded)
        {
            if (serializableData != null)
            {
                serializableData.isOpedEnded = isOpenEnded;
                shapeController.spline.isOpenEnded = isOpenEnded;
            }
        }


        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            if (serializableData == null)
            {
                serializableData = new LevelObjectMonolith.SerializableData();
                PointData pointData = default;
                for (int i = 0, n = shapeController.spline.GetPointCount(); i < n; i++)
                {
                    pointData = new PointData(i,
                        shapeController.spline.GetPosition(i),
                        shapeController.spline.GetTangentMode(i));

                    serializableData.monolithPointsData.Add(pointData);
                }
            }
        }


        private void UpdateSpline(List<PointData> splinePointsData)
        {
            MonolithRenderUtility.RenderMonolith(splinePointsData,
                                                 ref cornerObjects,
                                                 shapeController.polygonCollider,
                                                 shapeController.spline,
                                                 transform,
                                                 WeaponType.Sniper);
            OnPointsUpdate?.Invoke(this);
        }


        private void RefreshSpriteShape()
        {
            SpriteShape spriteShape = IngameData.Settings.monolith.CreateSpriteShape();

            if (spriteShape != null)
            {
                shapeController.spriteShape = spriteShape;
            }
        }

        #endregion



        #region IUpdatablePointStructure

        public event Action<IUpdatablePointStructure> OnPointsUpdate;


        public List<PointData> PointData
        {
            get
            {
                if (serializableData == null)
                {
                    return new List<PointData>();
                }

                return serializableData.monolithPointsData;
            }
            set
            {
                if (serializableData == null)
                {
                    serializableData = new LevelObjectMonolith.SerializableData();
                }

                serializableData.monolithPointsData = value;
            }
        }


        public void RefreshPoints(List<PointData> pointsData)
        {
            PointData = pointsData;
            UpdateSpline(pointsData);
        }


        public void RemovePoint(int pointIndex)
        {
            shapeController.spline.RemovePointAt(pointIndex);

            for (int i = pointIndex + 1, n = PointData.Count; i < n; i++)
            {
                PointData[i].pointIndex--;
            }

            PointData.RemoveAt(pointIndex);
            shapeController.BakeCollider();

            OnPointsUpdate?.Invoke(this);
        }

      

        public void InsertPoint(int pointIndex, Vector3 pointPosition)
        {
            pointPosition -= transform.position.SetZ(0.0f);

            shapeController.spline.InsertPointAt(pointIndex, pointPosition);
            shapeController.spline.SetTangentMode(pointIndex, defaultShapeTangentMode);
            PointData.Insert(pointIndex, new PointData(pointIndex, pointPosition, defaultShapeTangentMode));

            for (int i = pointIndex + 1, n = PointData.Count; i < n; i++)
            {
                PointData[i].pointIndex++;
            }

            RefreshPoints(PointData);

            OnPointsUpdate?.Invoke(this);
        }

        #endregion
    }
}
