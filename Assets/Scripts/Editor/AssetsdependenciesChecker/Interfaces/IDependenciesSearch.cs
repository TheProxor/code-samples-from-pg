using System.Collections.Generic;

namespace Drawmasters.Editor
{
    internal interface IDependenciesSearch
    {
        List<object> FindDependencies(string guid);
    }
}

