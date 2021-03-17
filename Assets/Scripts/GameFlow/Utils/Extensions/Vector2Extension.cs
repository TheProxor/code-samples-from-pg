using UnityEngine;


namespace Drawmasters
{
    public static class Vector2Extension
    {
        public static Vector2 Snap(this Vector2 vector, float snapDenominator)
        {
            return vector - new Vector2(vector.x % snapDenominator, vector.y % snapDenominator);
        }
    }
}
