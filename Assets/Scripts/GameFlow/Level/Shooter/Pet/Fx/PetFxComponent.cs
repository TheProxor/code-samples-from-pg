using Modules.General;
using Drawmasters.Levels;
using Drawmasters.Effects;
using UnityEngine;


namespace Drawmasters.Pets
{
    public class PetFxComponent : PetComponent
    {
        #region Fields

        private readonly PetWeaponSkinSettings petWeaponSkinSettings;

        private EffectHandler invokeMoveEffectHandler;

        #endregion



        #region Class lifecycle

        public PetFxComponent()
        {
            petWeaponSkinSettings = IngameData.Settings.pets.weaponSkinSettings;
        }

        #endregion



        #region Methods

        public override void Initialize(Pet mainPetValue)
        {
            base.Initialize(mainPetValue);

            InitializeAnimationFxPlayers();

            if (mainPet != null)
            {
                mainPet.OnPostPetSkinChange += MainPet_OnPostPetSkinChange;
            }

            PetShootComponent.OnShooted += PetShootComponent_OnShooted;
            PetInvokeComponent.OnPreInvokePetForLevel += PetInvokeComponent_OnPreInvokePetForLevel;
            PetInvokeComponent.OnShouldInvokePetForLevel += PetInvokeComponent_OnShouldInvokePetForLevel;
        }

        private void MainPet_OnPostPetSkinChange()
        {
            DeinitializeAnimationFxPlayers();
            InitializeAnimationFxPlayers();
        }


        public override void Deinitialize()
        {
            PetShootComponent.OnShooted -= PetShootComponent_OnShooted;
            PetInvokeComponent.OnPreInvokePetForLevel -= PetInvokeComponent_OnPreInvokePetForLevel;
            PetInvokeComponent.OnShouldInvokePetForLevel -= PetInvokeComponent_OnShouldInvokePetForLevel;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            EffectManager.Instance.ReturnHandlerToPool(invokeMoveEffectHandler);

            DeinitializeAnimationFxPlayers();

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void PetShootComponent_OnShooted(Pet anotherPet, Vector3 targetPosition)
        {
            if (mainPet != anotherPet)
            {
                return;
            }

            string attackFxKey = petWeaponSkinSettings.FindPetAttackFxKey(mainPet.Type);
            EffectManager.Instance.PlaySystemOnce(attackFxKey, mainPet.CurrentSkinLink.CurrentPosition, default, mainPet.CurrentSkinLink.transform);
        }


        private void PetInvokeComponent_OnPreInvokePetForLevel(Pet anotherPet)
        {
            if (mainPet != anotherPet)
            {
                return;
            }

            string fxKey = petWeaponSkinSettings.FindPetInvokeFxKey(mainPet.Type);
            invokeMoveEffectHandler = EffectManager.Instance.CreateSystem(fxKey, true, default, default, mainPet.CurrentSkinLink.transform, TransformMode.Local);
        }


        private void PetInvokeComponent_OnShouldInvokePetForLevel(Pet anotherPet)
        {
            if (mainPet != anotherPet)
            {
                return;
            }

            EffectManager.Instance.ReturnHandlerToPool(invokeMoveEffectHandler);
        }


        private void InitializeAnimationFxPlayers()
        {
            if (AllowWorkWithPet)
            {
                foreach (var player in mainPet.CurrentSkinLink.AnimationEffectPlayers)
                {
                    player.Initialize();
                }
            }
        }


        private void DeinitializeAnimationFxPlayers()
        {
            if (AllowWorkWithPet)
            {
                foreach (var player in mainPet.CurrentSkinLink.AnimationEffectPlayers)
                {
                    player.Deinitialize();
                }
            }
        }

        #endregion
    }
}
