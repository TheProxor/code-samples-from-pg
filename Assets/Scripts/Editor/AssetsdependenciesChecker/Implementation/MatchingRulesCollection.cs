using System.Collections.Generic;


namespace Drawmasters.Editor
{
    internal class MatchingRulesCollection : IMatchingRule
    {
        #region Fields

        private readonly List<IMatchingRule> rules;

        #endregion



        #region Ctor

        public MatchingRulesCollection(params IMatchingRule[] rule)
        {
            rules = new List<IMatchingRule>(rule);
        }

        #endregion


        #region IMatchingRule

        public bool IsMatch(string guid)
        {
            foreach(var i in rules)
            {
                if (i.IsMatch(guid))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

    }
}

