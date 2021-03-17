using UnityEngine;


namespace Drawmasters
{
    public static class Vector3Extension
    {
        public static Vector3 Snap(this Vector3 vector, float snapDenominator)
        {
            return vector - new Vector3(vector.x % snapDenominator, vector.y % snapDenominator, vector.z % snapDenominator);
        }
    }
}
