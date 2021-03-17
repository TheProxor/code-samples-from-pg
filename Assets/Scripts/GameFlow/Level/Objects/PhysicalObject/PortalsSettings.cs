using UnityEngine;
using System;
using Sirenix.OdinInspector;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "PortalsSettings",
                    menuName = NamingUtility.MenuItems.IngameSettings + "PortalsSettings")]
    public class PortalsSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class ObjectsPortalData
        {
            public PhysicalLevelObjectShapeType shapeType = default;
            public PhysicalLevelObjectSizeType sizeType = default;

            [Tooltip("может ли объект телепортироваться")]
            public bool allowTeleport = default;

            [EnableIf("allowTeleport")]
            [Tooltip("оффсет выхода из портала")]
            public float exitOffset = default;

            [EnableIf("allowTeleport")]
            [Tooltip("минимальная скорость при вылете из портала")]
            public float minVelocityMagnitudeForPortalExit = default;

            [EnableIf("allowTeleport")]
            [Tooltip("максимальная скорость при вылете из портала (чтобы не зацикливать")]
            public float maxVelocityMagnitudeForPortalExit = default;
        }

        #endregion



        #region Fields

        public float delayForNextTeleportation = default;
        public float laserPortalExitOffset = default;

        [Header("Portal objects")]
        public float portalsCreateOffset = default;

        [Header("Physics")]
        [SerializeField] private ObjectsPortalData[] objectsPortalData = default;

        [Tooltip("после телепортации умножаем скорость объектов на этот коэффициент")]
        public float teleportedObjectsVelocityCoefficient = default;

        [Tooltip("оффсет выхода из портала для врагов")]
        public float levelTargetExitOffset = default;

        [Tooltip("после телепортации умножаем скорость врагов на этот коэффициент")]
        public float teleportedLevelTargertsVelocityCoefficient = default;
        [Tooltip("минимальная скорость при вылете из портала для врагов")]
        public float minTargetsVelocityMagnitudeForPortalExit = default;

        [Tooltip("оффсет выхода из портала для оторваных конечностей врагов")]
        public float levelTargetLimbsExitOffset = default;

        [SerializeField] private string[] allowedTeleportLimbs = default;

        public float minLevelTargetVelocityMagnitude = default;
        public float maxLevelTargetVelocityMagnitude = default;

        public float minLimbVelocityMagnitude = default;
        public float maxLimbVelocityMagnitude = default;

        public float minPortalWidthForCollision = default;

        #endregion



        #region Methods

        public bool AllowTeleport(PhysicalLevelObjectData objectData)
        {
            ObjectsPortalData data = FindObjectsPortalData(objectData);
            return data == null ? false : data.allowTeleport;
        }


        public bool AllowTeleportLimb(string boneName) => Array.Exists(allowedTeleportLimbs, e => e == boneName);


        public float MinVelocityMagnitudeForPortalExit(PhysicalLevelObjectData objectData)
        {
            ObjectsPortalData data = FindObjectsPortalData(objectData);
            return data == null ? default : data.minVelocityMagnitudeForPortalExit;
        }


        public float MaxVelocityMagnitudeForPortalExit(PhysicalLevelObjectData objectData)
        {
            ObjectsPortalData data = FindObjectsPortalData(objectData);
            return data == null ? default : data.maxVelocityMagnitudeForPortalExit;
        }


        public float FindExitOffset(PhysicalLevelObjectData objectData)
        {
            ObjectsPortalData data = FindObjectsPortalData(objectData);
            return data == null ? default : data.exitOffset;
        }


        private ObjectsPortalData FindObjectsPortalData(PhysicalLevelObjectData objectData)
        {
            ObjectsPortalData foundData = Array.Find(objectsPortalData, e => e.shapeType == objectData.shapeType && e.sizeType == objectData.sizeType);

            if (foundData == null)
            {
                CustomDebug.Log($"No portal data found for object {objectData} in {this}");
            }

            return foundData;
        }

        #endregion
    }
}
