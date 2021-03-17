using Modules.General.Abstraction;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IAbTestGenericService<TCommon, TMopub, TAds>
        where TCommon : IAbTestData
        where TMopub : IAbTestData
        where TAds : IAbTestData
    {
        TCommon CommonData { get; }

        TMopub MopubData { get; }

        TAds AdsData { get; }
    }
}

