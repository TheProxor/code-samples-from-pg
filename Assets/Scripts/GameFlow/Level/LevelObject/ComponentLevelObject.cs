using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class ComponentLevelObject : LevelObject
    {
        #region Properties

        public virtual bool ShouldResetComponentsOnReturn => true;

        #endregion



        #region Override methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            InitializeComponents();
        }

        public override void StartGame(GameMode mode, WeaponType weaponType, Transform levelTransform)
        {
            base.StartGame(mode, weaponType, levelTransform);

            EnableComponents();
        }


        public override void FinishGame()
        {
            DisableComponents();

            base.FinishGame();
        }


        protected override void OnShouldFinishReturnToInitialState()
        {
            base.OnShouldFinishReturnToInitialState();

            if (ShouldResetComponentsOnReturn)
            {
                DisableComponents();
                InitializeComponents();
                EnableComponents();
            }
        }

        #endregion



        #region Abstract methods

        protected abstract void InitializeComponents();


        protected abstract void EnableComponents();


        protected abstract void DisableComponents();

        #endregion
    }
}
