using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "New Physical Data",
                     menuName = NamingUtility.MenuItems.LevelObjectsData + "PhysicalData")]
    public class PhysicalLevelObjectData : ScriptableObject
    {
        #region Fields

        public PhysicalLevelObjectType type = default;
        public PhysicalLevelObjectSizeType sizeType = default;
        public PhysicalLevelObjectShapeType shapeType = default;

        #endregion


        #region Methods

        public override bool Equals(object other)
        {
            bool result = default;

            if (other == null)
            {
                return result;
            }

            PhysicalLevelObjectData data = other as PhysicalLevelObjectData;

            if (data == null)
            {
                return result;
            }

            result = (data.type == type &&
                      data.sizeType == sizeType &&
                      data.shapeType == shapeType);
            
            return result;
        }
        

        public override int GetHashCode()
        {
            int result = default;

            result |= (int)type      << sizeof(byte) * 0;
            result |= (int)sizeType  << sizeof(byte) * 1;
            result |= (int)shapeType << sizeof(byte) * 2;

            return result;
        }

        #endregion
    }
}
