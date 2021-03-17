using UnityEngine;

namespace Drawmasters.Lifecycle
{
    public static class GameObjectExtension
    {
        public static string FullHierarchyName(this GameObject gameObject) =>
            FindNameRecursively(gameObject.transform);

        private static string FindNameRecursively(Transform transform)
        {
            string result;

            if (transform.parent != null)
            {
                result = $"{FindNameRecursively(transform.parent)}->{transform.name}";
            }
            else
            {
                result = transform.name;
            }

            return result;
        }
    }
}