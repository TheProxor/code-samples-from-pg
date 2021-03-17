namespace Drawmasters.Levels
{
    public interface ITeleportable
    {
        bool TryTeleport(PortalObject enteredPortal, PortalObject exitPortal);
    }
}
