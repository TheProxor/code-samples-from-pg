using System;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters
{
    [Serializable]
    public abstract class LeaderBoardItem: PolymorphicObject
    {
        #region Filds

        [SerializeField] protected string id = default;
        [SerializeField] protected string nickName = default;
        [SerializeField] protected float skullsCount = default;
        [SerializeField] protected ShooterSkinType shooterSkinType = default;
        [SerializeField] protected bool isBloked;
        #endregion



        #region Property

        public bool IsActive { get; set; }

        public bool IsBloked => isBloked; 
        
        public abstract LeaderBordItemType ItemType { get; }
        
        
        public virtual ShooterSkinType SkinType => 
            shooterSkinType;
        
        
        public virtual string Id => id;
        
        
        public virtual string NickName {
            get => nickName;
            set => nickName = value;
        }
        
        
        public virtual float SkullsCount {
            get => skullsCount;
            set => skullsCount = value;
        }

        #endregion



        #region Class lifecycle

        protected LeaderBoardItem(string _id, string _nickName, ShooterSkinType _skinType)
        {
            id = _id;
            nickName = _nickName;
            shooterSkinType = _skinType;
        }
        

        protected LeaderBoardItem()
        {
            id = string.Empty;
            nickName = string.Empty;
            shooterSkinType = ShooterSkinType.Archer;
        }

        #endregion
        
        
        
        #region Public methods

        public virtual void AddSkulls(float skulls)
        {
            skullsCount += skulls;
        }
        
        
        public virtual void Reset()
        {
            skullsCount = 0;
        }
        
        #endregion
    }
}