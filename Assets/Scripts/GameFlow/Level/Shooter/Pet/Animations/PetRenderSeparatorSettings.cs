using System;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "PetRenderSeparatorSettings",
                  menuName = NamingUtility.MenuItems.IngameSettings + "PetRenderSeparatorSettings")]
    public class PetRenderSeparatorSettings : ScriptableObject
    {
        #region Helpers
        
        [Serializable]
        public class ShooterData
        {
            public SkinSkeletonType shooterSkinType = default;
            public int[] renderOrders;
        }
        
        [Serializable]
        public class Data
        {
#pragma warning disable 0414

            // for reflection only
            public SkeletonDataAsset dataAsset = default;

#pragma warning restore 0414

            public PetSkinType type = default;
            [SpineAnimation(dataField = "dataAsset")] public string animationName = default;

            public ShooterData[] shooterDatas;
        }
        
        #endregion

        
        
        #region Fields

        [SerializeField] private Data[] animations = default;
        
        #endregion


        
        #region Methods

        public int[] FindAnimation(PetSkinType petType, SkinSkeletonType shooterType, string animationName)
        {
            int[] result = null;
            
            Data findAnimation = animations.Find(x => x.type == petType && x.animationName.Equals(animationName));
            
            if (findAnimation != null)
            {
                ShooterData shooterData = findAnimation.shooterDatas.Find(x => x.shooterSkinType == shooterType);

                if (shooterData != null)
                {
                    result = shooterData.renderOrders;
                }
            }
            return result;
        } 
        
        #endregion
    }
}
