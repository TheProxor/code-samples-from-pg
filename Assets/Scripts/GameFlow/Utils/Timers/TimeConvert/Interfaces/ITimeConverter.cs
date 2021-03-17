namespace Drawmasters.Utils.UiTimeProvider.Interfaces
{
    public interface ITimeConverter<TConvertedType>
    {
        TConvertedType Convert(RealtimeTimer timer);
    }
}