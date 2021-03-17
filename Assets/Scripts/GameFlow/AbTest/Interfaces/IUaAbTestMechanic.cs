namespace Drawmasters.Interfaces
{
    public interface IUaAbTestMechanic
    {
        bool IsMechanicAvailable { get; }
        
        bool WasAvailabilityChanged { get; }

        void ChangeMechanicAvailability(bool isAvailable);

        void ResetAvailability();
    }
}

