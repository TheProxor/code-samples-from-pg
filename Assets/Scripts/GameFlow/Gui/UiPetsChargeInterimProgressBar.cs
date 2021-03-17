using Drawmasters.Pets;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Ui
{
    public class UiPetsChargeInterimProgressBar : UiProgressBar
    {
        #region Fields

        private PetSkinType petSkinType;
        private PetsChargeController petsChargeController;

        #endregion


        
        #region Class Lifecycle

        public override void Initialize()
        {
            base.Initialize();

            petsChargeController = GameServices.Instance.PetsService.ChargeController;
            petsChargeController.OnChargeInterimPoints += ChargeController_OnChargeInterimPoints;

            SetBounds(petsChargeController.MinChargePoints, petsChargeController.MaxChargePoints);
            
            float petsChargePoints = petsChargeController.CurrentChargePointsCount;
            
            UpdateProgress(petsChargePoints, petsChargePoints);

        }


        public override void Deinitialize()
        {
            if (petsChargeController != null)
            {
                petsChargeController.OnChargeInterimPoints -= ChargeController_OnChargeInterimPoints;
            }

            base.Deinitialize();
        }

        #endregion



        #region Event Handlers
        
        private void ChargeController_OnChargeInterimPoints(PetSkinType chargedPetSkinType, float recievedPoints)
        {
            if (this == null)
            {
                CustomDebug.Log("Attempt to handle callback for null object");
            }

            if (petSkinType != chargedPetSkinType || recievedPoints > 0)
            {
                return;
            }
            
            float petsChargePoints = petsChargeController.CurrentChargePointsCount;

            UpdateProgress(petsChargePoints);
        }

        #endregion
    }
}
