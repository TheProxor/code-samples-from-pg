using UnityEngine;


namespace Drawmasters.Monolith
{
    public class CornerInfo
    {
        #region Properties

        public CornerFormType FormType { get; private set; }

        public Vector2 ParentPosition { get; private set; }

        public float ParentAngle { get; private set; }

        public float CornerAngle { get; private set; }

        public Sprite CornerSprite { get; private set; }

        public float Rotation => MonolithUtility.CornerAngle(FormType,
                                                             ParentAngle,
                                                             CornerAngle);
        
        public Vector2 Position => ParentPosition + Offset;

        public float TileOffset { get; private set; }

        private Vector2 Offset => MonolithUtility.CornerOffset(this);
        
        #endregion



        #region Lifecycle

        public CornerInfo(CornerFormType formType,
                          Vector2 parentPoint,
                          float parentAngle,
                          float cornerAngle,
                          WeaponType weaponType,
                          float offset)
        {
            FormType = formType;
            ParentPosition = parentPoint;
            ParentAngle = parentAngle;
            CornerAngle = cornerAngle;
            
            float clampedAngle = Mathf.Abs(CornerAngle);
            clampedAngle = Mathf.Ceil(clampedAngle);
            CornerSprite = IngameData.Settings.monolith.GetCornerSprite(formType, (int)clampedAngle);

            TileOffset = offset;
        }

        #endregion
    }
}

