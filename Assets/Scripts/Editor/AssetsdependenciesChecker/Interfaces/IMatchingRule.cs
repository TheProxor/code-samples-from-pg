namespace Drawmasters.Editor
{
    internal interface IMatchingRule
    {
        bool IsMatch(string guid);
    }
}
