using DG.Tweening;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class WeaponAnimatedDestroy : WeaponDestroy
    {
        #region Overrided methods

        public override void Enable()
        {
            base.Enable();

            sourceLevelObject.MainCollider2D.enabled = true;
        }

        public override void Disable()
        {
            base.Disable();

            DOTween.Kill(this);
        }


        protected override void PerformObjectDestroy()
        {
            sourceLevelObject.MainCollider2D.enabled = false;
            
            FactorAnimation animation = IngameData.Settings.bonusLevelSettings.scaleAnimation;
            
            animation.Play(
                SetScale,
                this, 
                () => base.PerformObjectDestroy());
        }

        #endregion



        #region Private methods

        private void SetScale(float x)
            => sourceLevelObject.transform.localScale = new Vector3(x, x, x);

        #endregion
    }
}
