using Drawmasters.Levels;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Pets
{
    public class PetCurrencyAbsorbComponent : PetComponent
    {
        #region Fields

        private readonly CurrencyObjectsLevelController currencyObjectsLevelController;

        #endregion



        #region Class lifecycle

        public PetCurrencyAbsorbComponent()
        {
            currencyObjectsLevelController = GameServices.Instance.LevelControllerService.CurrencyObjectsLevelController;
        }

        #endregion



        #region Methods

        public override void Initialize(Pet _pet)
        {
            base.Initialize(_pet);

            PetInvokeComponent.OnShouldInvokePetForLevel += PetInvokeComponent_OnShouldInvokePetForLevel;
        }


        public override void Deinitialize()
        {
            PetInvokeComponent.OnShouldInvokePetForLevel -= PetInvokeComponent_OnShouldInvokePetForLevel;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void PetInvokeComponent_OnShouldInvokePetForLevel(Pet anotherPet)
        {
            if (mainPet == anotherPet)
            {
                Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            }
        }


        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.AllTargetsHitted)
            {
                currencyObjectsLevelController.AbsorbAllCurrencyObjects(mainPet.CurrentSkinLink);
            }
        }

        #endregion
    }
}
