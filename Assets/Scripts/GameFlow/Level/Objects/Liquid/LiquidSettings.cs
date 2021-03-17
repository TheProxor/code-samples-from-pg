using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "LiquidSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "LiquidSettings")]
    public class LiquidSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class AdditionalDragData
        {
            public CollidableObjectType type = default;

            public float additionalLinearDrag = default;
            public float additionalAngularDrag = default;
        }

        #endregion



        #region Fields

        [Header("Spring")]

        [Tooltip("Айдловый импульс для рандомной точки на сплайне.")]
        public float idleImpulsMagnitude = 0.02f;
        public float idleImpulsPeriod = 0.02f;

        [Tooltip("Множитель импульса для попавших в воду объектов (регулирует придание силы в целом).")]
        public float falledObjectImpulsMultiplier = 0.02f;

        [Tooltip("Это коэфициент жесткости в формуле F = -k * x.")]
        [Range(0.00001f, 0.5f)]
        public float spring = 0.02f;
        [Tooltip("Коэфициент затухания")]
        [Range(0.00001f, 0.5f)]
        public float damping = 0.04f;
        [Tooltip("Коэфициент распространения для соседних точек")]
        [Range(0.00001f, 0.2f)]
        public float spread = 0.05f;
        [Tooltip("Количество проходов для рассчетов распространения соседних точек")]
        [Range(0, 20)]
        public int passesCount = 8;

        public float distanceBetweenPoints = 64.0f / 5.0f;
        public float commonBezieMagnitude = 10.0f;

        [SerializeField] private AdditionalDragData[] dragData = default;

        [Header("Level objects")]
        public float fullyCoveredObjectsDestoyDelay = default;

        [Header("Level target")]
        public float levelTargetDestoyDelay = default;

        [Header("Visual")]
        [SerializeField] private LiquidLevelObjectVisual[] visual = default;

        #endregion



        #region Methods

        public LiquidLevelObjectVisual FindVisual(LiquidLevelObjectType type)
        {
            LiquidLevelObjectVisual foundData = Array.Find(visual, element => element.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"No visual data found for type {type} in {this}");
            }

            return foundData;
        }


        public float FindLinearDrag(CollidableObjectType type)
        {
            AdditionalDragData foundData = FindData(type);

            return (foundData == null) ? default : foundData.additionalLinearDrag;
        }


        public float FindAngularDrag(CollidableObjectType type)
        {
            AdditionalDragData foundData = FindData(type);

            return (foundData == null) ? default : foundData.additionalAngularDrag;
        }


        private AdditionalDragData FindData(CollidableObjectType type)
        {
            AdditionalDragData foundData = Array.Find(dragData, element => element.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"No data found for type {type} in {this}");
            }

            return foundData;
        }

        #endregion
    }
}
