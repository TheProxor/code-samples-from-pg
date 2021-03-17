using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "CommonPhysicalObjectsSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "CommonPhysicalObjectsSettings")]
    public class CommonPhysicalObjectsSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class CorrosiveData
        {
            public PhysicalLevelObjectType type = default;
            [Tooltip("Множитель для объема, значение определит длительность плавнения объектов кислотой")]
            public float acidCorrosiveVolumeMultiplier = default;
            [Tooltip("Множитель для объема, значение определит длительность плавнения объектов кислотой")]
            public float laserCorrosiveVolumeMultiplier = default;
        }


        [Serializable]
        private class MaterialData
        {
            public PhysicalLevelObjectType type = default;
            [Tooltip("Вес одного тайла. Участвует в рассчете массы")]
            public float tileWeight = default;
            [Tooltip("Коэфициент прочности. Участвует в рассчете прочности")]
            public float strengthCoefficient = default;
        }


        [Serializable]
        private class SizeData
        {
            public PhysicalLevelObjectSizeType type = default;
            [Tooltip("Объем. От объема зависят масса и прочность. От объема зависит время плавнения кислотой")]
            public float volume = default;
        }


        [Serializable]
        private class Data
        {
            public PhysicalLevelObjectType type = default;
            public PhysicalLevelObjectSizeType sizeType = default;
            public PhysicalLevelObjectShapeType shapeType = default;

            [Header("Physics")]
            [Tooltip("Локальный коэфициент для объема")]
            public float volumeMultiplier = default;
            [Tooltip("Локальный коэфициент для массы")]
            public float massMultiplier = default;
            [Tooltip("Локальный коэфициент для прочности")]
            public float strengthMultiplier = default;

            [Header("Damage")]
            public int hitsForDestroy = default;
            [Tooltip("Умножаем импульс на это значение и получаем урон по персу(кроме специальных случаев)")]
            public float damageConversionImpulsMultiplier = default;
            [Tooltip("Умножаем импульс на это значение и получаем урон по персу для случая для отрыва всех конечностей")]
            public float allLimbsChopOffDamageConversionImpulsMultiplier = default;

            [Header("Visual")]
            public Sprite[] sprites = default;
        }


        [Serializable]
        private class SoundData
        {
            public PhysicalLevelObjectType type = default;
            public float impuls = default;
            public float volume = default;
        }

        #endregion



        #region Fields

        [SerializeField] private MaterialData[] materialData = default;
        [SerializeField] private SizeData[] sizeData = default;
        [SerializeField] private Data[] objectsData = default;

        public float gravityScale = default;

        [Header("Acid")]
        [Range(0.0f, 1.0f)]
        public float corrosiveFactorToDisableObjectVisual = default;
        [Header("Laser")]
        [Range(0.0f, 1.0f)]
        public float laserFactorToDisableObjectVisual = default;

        [SerializeField] private CorrosiveData[] corrosiveData = default;
        [SerializeField] private SoundData[] soundData = default;

        [Header("Common sounds configs")]
        public float velocityForHitSound = 2f;

        [Header("Pets")]
        public float petsImpulsMagnitude = default;

        #endregion



        #region Methods

        public float FindCorrosiveMultiplier(PhysicalLevelObjectData data)
        {
            CorrosiveData foundData = Array.Find(corrosiveData, element => element.type == data.type);
            return (foundData == null) ? 1.0f : foundData.acidCorrosiveVolumeMultiplier;
        }


        public float FindLaserCorrosiveMultiplier(PhysicalLevelObjectData data)
        {
            CorrosiveData foundData = Array.Find(corrosiveData, element => element.type == data.type);
            return (foundData == null) ? 1.0f : foundData.laserCorrosiveVolumeMultiplier;
        }


        public float CalculateVolume(PhysicalLevelObjectData data)
        {
            SizeData foundSizeData = FindSizeData(data.sizeType);
            float volume = (foundSizeData == null) ? default : foundSizeData.volume;

            Data foundData = FindData(data);
            float volumeMultiplier = (foundData == null) ? default : foundData.volumeMultiplier;

            return volume * volumeMultiplier;
        }


        public float CalculateMass(PhysicalLevelObjectData data)
        {
            float volume = CalculateVolume(data);

            MaterialData foundMaterialData = FindMaterialData(data.type);
            float density = (foundMaterialData == null) ? default : foundMaterialData.tileWeight;

            Data foundData = FindData(data);
            float massMultiplier = (foundData == null) ? default : foundData.massMultiplier;

            return density * volume * massMultiplier;
        }


        public float CalculateStrength(PhysicalLevelObjectData data)
        {
            float mass = CalculateMass(data);

            MaterialData foundMaterialData = FindMaterialData(data.type);
            float strengthCoefficient = (foundMaterialData == null) ? default : foundMaterialData.strengthCoefficient;

            Data foundData = FindData(data);
            float strengthMultiplier = (foundData == null) ? default : foundData.strengthMultiplier;

            return mass * strengthCoefficient * strengthMultiplier;
        }


        public Sprite[] GetSprites(PhysicalLevelObjectData data)
        {
            Data foundData = Array.Find(objectsData, element => element.sizeType == data.sizeType &&
                                                     element.type == data.type &&
                                                     element.shapeType == data.shapeType);

            if (foundData == null)
            {
                CustomDebug.Log($"No data for physical object with type {data.type} and size type {data.sizeType}  and shape type {data.shapeType} don't exist");
            }

            Sprite[] result = (foundData == null) ? default : foundData.sprites;

            return result;
        }


        public int GetMaxHitsForDestroy(PhysicalLevelObjectData data)
        {
            Data foundData = FindData(data);
            return (foundData == null) ? default : foundData.hitsForDestroy;
        }


        public float GetDamageImpulsMultiplier(PhysicalLevelObjectData data)
        {
            Data foundData = FindData(data);
            return (foundData == null) ? default : foundData.damageConversionImpulsMultiplier;
        }


        public float GetAllLimbsChopOffDamageImpulsMultiplier(PhysicalLevelObjectData data)
        {
            Data foundData = FindData(data);
            return (foundData == null) ? default : foundData.allLimbsChopOffDamageConversionImpulsMultiplier;
        }


        public string GetImpulsSoundKey(PhysicalLevelObjectData data)
        {
            string result = default;

            if (data.shapeType == PhysicalLevelObjectShapeType.Box ||
                data.shapeType == PhysicalLevelObjectShapeType.Plank ||
                data.shapeType == PhysicalLevelObjectShapeType.Spikes)
            {
                result = SoundGroupKeys.RandomBoxFallDownKeys;
            }

            if (data.shapeType == PhysicalLevelObjectShapeType.Sphere)
            {
                result = (data.sizeType == PhysicalLevelObjectSizeType.Big) ?
                         SoundGroupKeys.RandomBigSphereFallDownKeys : SoundGroupKeys.RandomSmallSphereFallDownKeys;
            }

            return result;
        }


        public float GetImpulsSoundVolume(float impuls, PhysicalLevelObjectData objectData)
        {
            float result = 1.0f;

            foreach (var data in soundData)
            {
                if (impuls > data.impuls &&
                    data.type == objectData.type)
                {
                    result = data.volume;
                }
            }

            return result;
        }


        private Data FindData(PhysicalLevelObjectData data)
        {
            Data foundData = Array.Find(objectsData, element => element.sizeType == data.sizeType &&
                                                     element.type == data.type &&
                                                     element.shapeType == data.shapeType);

            if (foundData == null)
            {
                CustomDebug.Log($"No data for physical object with type {data.type} and size type {data.sizeType}  and shape type {data.shapeType} don't exist");
            }

            return foundData;
        }


        private MaterialData FindMaterialData(PhysicalLevelObjectType type)
        {
            MaterialData foundData = Array.Find(materialData, element => element.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"No data for physical object with type {type} don't exist");
            }

            return foundData;
        }


        private SizeData FindSizeData(PhysicalLevelObjectSizeType type)
        {
            SizeData foundData = Array.Find(sizeData, element => element.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"No data for physical object with type {type} don't exist");
            }

            return foundData;
        }

        #endregion
    }
}
