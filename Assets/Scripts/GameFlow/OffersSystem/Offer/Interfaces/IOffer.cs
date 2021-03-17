namespace Drawmasters.OffersSystem
{
    public interface IOffer
    {
        string OfferType { get; }
        
        string OfferId { get; }
        
        float DurationTime { get; }
        
        string InAppId { get; }
        
        bool IsMechanicAvailable { get; }
        
        bool IsActive { get; }
        
        bool IsAllTriggersActive { get; }
        
        float OfferCooldownTime { get; }
    }
}