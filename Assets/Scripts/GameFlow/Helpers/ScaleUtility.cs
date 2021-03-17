using UnityEngine;


public static class ScaleUtility
{
    #region Fields

    public const float MinimalScaleMultiplier = 0.001f;

    public static readonly Vector3 MinimalScale = Vector3.one * MinimalScaleMultiplier;

    #endregion


    #region Methods

    public static void ChangeClampedScale<T>(T monoBehaviour, Vector3 scale) 
        where T : MonoBehaviour
    {
        if (monoBehaviour != null)
        {
            ChangeClampedScale(monoBehaviour.transform, scale);
        }
    }


    public static void ChangeClampedScale(GameObject workGameobject, Vector3 scale)
    {
        if (workGameobject != null)
        {
            ChangeClampedScale(workGameobject.transform, scale);
        }
    }


    public static void ChangeClampedScale(Transform workTransform, Vector3 scale)
    {
        Vector3 clampedScale = new Vector3(Mathf.Max(MinimalScaleMultiplier, scale.x),
                                            Mathf.Max(MinimalScaleMultiplier, scale.y),
                                            Mathf.Max(MinimalScaleMultiplier, scale.z));

        workTransform.localScale = clampedScale;
    }

    #endregion
}
