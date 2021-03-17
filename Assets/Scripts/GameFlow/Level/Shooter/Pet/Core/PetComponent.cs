using Drawmasters.Levels;


namespace Drawmasters.Pets
{
    public abstract class PetComponent
    {
        #region Fields

        protected Pet mainPet;

        #endregion



        #region Proeprties

        protected bool AllowWorkWithPet =>
            mainPet != null &&
            mainPet.IsPetExists &&
            mainPet.CurrentSkinLink != null;

        #endregion



        #region Methods

        public virtual void Initialize(Pet mainPetValue)
        {
            mainPet = mainPetValue;
        }


        public virtual void Deinitialize()
        {
        }

        #endregion
    }
}
