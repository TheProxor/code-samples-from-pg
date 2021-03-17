using Drawmasters.Effects;
using Drawmasters.Levels;


namespace Drawmasters.Pets
{
    public class PetTrailComponent : PetComponent
    {
        #region Fields

        private string trailName;
        private EffectHandler effectHandler;

        #endregion



        #region Methods

        public override void Initialize(Pet mainPetValue)
        {
            base.Initialize(mainPetValue);

            if (mainPetValue.Type != PetSkinType.None)
            {
                trailName = IngameData.Settings.pets.weaponSkinSettings.FindPetTrailFxsKey(mainPetValue.Type);
            }

            PetInvokeComponent.OnShouldInvokePetForLevel += PetInvokeComponent_OnShouldInvokePetForLevel;
        }


        public override void Deinitialize()
        {
            PetInvokeComponent.OnShouldInvokePetForLevel -= PetInvokeComponent_OnShouldInvokePetForLevel;

            EffectManager.Instance.ReturnHandlerToPool(effectHandler);

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void PetInvokeComponent_OnShouldInvokePetForLevel(Pet pet)
        {
            if (pet == mainPet)
            {
                if (!string.IsNullOrEmpty(trailName))
                {
                    effectHandler = EffectManager.Instance.CreateSystem(trailName,
                                                                        true,
                                                                        mainPet.CurrentSkinLink.CurrentPosition,
                                                                        default,
                                                                        mainPet.CurrentSkinLink.transform,
                                                                        TransformMode.World,
                                                                        false);
                    if (effectHandler != null)
                    {
                        effectHandler.Play(withClear: false);
                    }
                }
            }
        }

        #endregion
    }
}

