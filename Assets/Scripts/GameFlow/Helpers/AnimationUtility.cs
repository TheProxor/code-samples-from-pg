using UnityEngine;


public static class AnimationUtility
{
    #region Properties

    public static AnimationCurve LinearCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f, 0.0f, 0.0f), 
                                                                  new Keyframe(1.0f, 1.0f, 0.0f, 0.0f));

    #endregion
}
