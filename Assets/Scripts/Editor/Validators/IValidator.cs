namespace Drawmasters.Editor
{
    public interface IValidator
    {
        bool Validate { get; }

        string SuccessfulValidateMessage { get; }
        string FailValidateMessage { get; }
    }
}
