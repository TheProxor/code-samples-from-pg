using System;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Statistics.Data;
using Drawmasters.Levels;
using Drawmasters.Interfaces;
using Drawmasters.AbTesting;


namespace Drawmasters.Pets
{
    public class PetsChargeController 
    {
        #region Fields

        public static IUaAbTestMechanic UaAbAlwaysCharged { get; } = new CommonMechanicAvailability(PrefsKeys.Dev.Pet.AlwaysCharged);

        public event Action<PetSkinType> OnCharged;
        public event Action<PetSkinType, float> OnChargePointsApplied;
        public event Action<PetSkinType, float> OnChargeInterimPoints;

        private readonly PetLevelSettings petLevelSettings;

        private float recievedChargePoints;

        #endregion



        #region Properties

        public bool IsActive =>
            CurrentPetSkinType != PetSkinType.None;
        
        
        public float CurrentChargePointsCount =>
            GetChargePoints(CurrentPetSkinType);


        public float MinChargePoints => 0.0f;


        public bool IsCurrentPetCharged =>
            IsPetCharged(CurrentPetSkinType);


        public float MaxChargePoints =>
            petLevelSettings.maxChargePoints;


        public PetSkinType CurrentPetSkinType =>
            PlayerData.CurrentPetSkin;
        
        
        public float RecievedChargePointsOnLevel
        {
            get => recievedChargePoints;

            private set
            {
                recievedChargePoints = value;
                
                OnChargeInterimPoints?.Invoke(CurrentPetSkinType, recievedChargePoints);
            }
            
        }
        
        
        private PlayerData PlayerData =>
            GameServices.Instance.PlayerStatisticService.PlayerData;

        #endregion



        #region Ctor

        public PetsChargeController(ILevelControllerService levelControllerService)
        {
            petLevelSettings = IngameData.Settings.pets.levelSettings;

            RecievedChargePointsOnLevel = MinChargePoints;

            levelControllerService.Target.OnTargetHitted += TargetController_OnTargetHitted;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;

            LevelProgressObserver.OnLevelStateChanged += LevelStateObserver_OnLevelStateChanged;
        }

        #endregion



        #region Methods

        public void ResetCharge()
        {
            PetSkinType petSkinType = CurrentPetSkinType;

            bool wasPetCharged = IsPetCharged(petSkinType);

            PlayerData.SetPetCharge(petSkinType, MinChargePoints);
            RecievedChargePointsOnLevel = MinChargePoints;

            OnChargePointsApplied?.Invoke(petSkinType, MinChargePoints);
        }


        public void ChargeImmediately()
        {
            RecievedChargePointsOnLevel = MaxChargePoints;

            ApplyChargePoints();
        }


        public void AddChargePoints(float value) =>
            RecievedChargePointsOnLevel += value;


        public float GetChargePoints(PetSkinType petSkinType) =>
            PlayerData.GetPetCharge(petSkinType);


        public bool IsPetCharged(PetSkinType petSkinType)
        {
            bool isUaAbAlwaysCharged = UaAbAlwaysCharged.WasAvailabilityChanged &&
                                       UaAbAlwaysCharged.IsMechanicAvailable;

            if (isUaAbAlwaysCharged)
            {
                return true;
            }

            return PlayerData.GetPetCharge(petSkinType) >= MaxChargePoints;
        }


        public void ApplyChargePoints()
        {
            // TODO: DO NOT DO THIS SHIT. CREATE INITIALIZE EVENT. TO DMITRY S
            PetSkinType petSkinType = CurrentPetSkinType;

            bool wasPetCharged = IsPetCharged(petSkinType);
            PlayerData.SetPetCharge(petSkinType, CurrentChargePointsCount + RecievedChargePointsOnLevel);

            OnChargePointsApplied?.Invoke(petSkinType, RecievedChargePointsOnLevel);

            if (!wasPetCharged && IsPetCharged(petSkinType))
            {
                OnCharged?.Invoke(petSkinType);
            }

            RecievedChargePointsOnLevel = MinChargePoints;
        }

        #endregion



        #region Event Handlers

        private void TargetController_OnTargetHitted(LevelTargetType hittedType)
        {
            if (hittedType.IsEnemy())
            {
                RecievedChargePointsOnLevel++;
            }
        }


        // TODO: rewrite it. We should accumulate and reset points like in LevelExtraCurrencyController.cs
        private void Level_OnLevelStateChanged(LevelState state)
        {
            bool shouldApplyChargePoints = state == LevelState.AllTargetsHitted &&
                                           GameServices.Instance.LevelEnvironment.Context.Mode.IsDrawingMode();
            if (shouldApplyChargePoints)
            {
                ApplyChargePoints();
            }
            else
            {
                RecievedChargePointsOnLevel = MinChargePoints;
            }
        }


        private void LevelStateObserver_OnLevelStateChanged(LevelResult result)
        {
            if (result == LevelResult.Reload)
            {
                RecievedChargePointsOnLevel = MinChargePoints;
            }
        }

        #endregion
    }
}
