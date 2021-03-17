using UnityEngine;
using Spine.Unity;
using System.Collections.Generic;
using Drawmasters.Effects;


namespace Drawmasters.Levels
{
    public class SellerLevelObject : ComponentLevelObject
    {
        #region Fields

        [SerializeField] private IdleEffect[] idleEffects = default;

        [SerializeField] private SkeletonAnimation skeletonAnimation = default;

        [SerializeField][SpineSkin] private string mainSkin = default;
        [SerializeField][SpineSkin] private string[] allowedWeaponSkins = default;

        private List<LevelObjectComponentTemplate<SellerLevelObject>> components;

        #endregion



        #region Properties

        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        #endregion



        #region Methods

        public override void StartGame(GameMode mode, WeaponType weaponType, Transform levelTransform)
        {
            base.StartGame(mode, weaponType, levelTransform);

            foreach (var effect in idleEffects)
            {
                effect.CreateAndPlayEffect();
            }
        }


        public override void FinishGame()
        {
            foreach (var effect in idleEffects)
            {
                effect.StopEffect();
            }

            base.FinishGame();
        }


        protected override void InitializeComponents()
        {
            if (components == null)
            {
                components = new List<LevelObjectComponentTemplate<SellerLevelObject>>
                {
                    new SellerSkinComponent(mainSkin, allowedWeaponSkins)
                };
            }

            foreach (var component in components)
            {
                component.Initialize(this);
            }
        }


        protected override void EnableComponents()
        {
            foreach (var component in components)
            {
                component.Enable();
            }
        }


        protected override void DisableComponents()
        {
            foreach (var component in components)
            {
                component.Disable();
            }
        }
        
        #endregion
    }
}
