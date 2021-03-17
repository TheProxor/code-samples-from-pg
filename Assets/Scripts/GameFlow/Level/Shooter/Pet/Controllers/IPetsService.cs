namespace Drawmasters.Pets
{
    public interface IPetsService
    {
        PetsChargeController ChargeController { get; }

        PetsInvokeController InvokeController { get; }

        PetsRestoreController RestoreController { get; }

        PetsTutorialController TutorialController { get; }
    }
}
