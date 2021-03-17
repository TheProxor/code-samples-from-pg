namespace Drawmasters.Editor
{
    internal class SimpleDependenciesSearch : IMatchingRule
    {
        #region Fields

        private readonly IMatchingRule ruleCollection;

        #endregion



        #region Ctor

        public SimpleDependenciesSearch(params IMatchingRule[] rules)
        {
            ruleCollection = new MatchingRulesCollection(rules);
        }

        #endregion



        #region IMatchingRule

        public bool IsMatch(string guid) =>
            ruleCollection.IsMatch(guid);

        #endregion
    }
}

