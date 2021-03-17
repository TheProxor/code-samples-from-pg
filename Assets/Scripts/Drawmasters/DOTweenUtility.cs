using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;


/// <summary>
/// Move rigidbody2D path. Actually works with Vector2[].
/// </summary>
public static class DOTweenUtility
{
    public static TweenerCore<Vector3, Path, PathOptions> DOPath(
               this Rigidbody2D target, Vector2[] path2D, float duration, PathType pathType = PathType.Linear,
               PathMode pathMode = PathMode.Full3D, int resolution = 10, Color? gizmoColor = null
           )
    {
        Vector3[] path = Array.ConvertAll(path2D, (v) => v.ToVector3());
        
        if (resolution < 1) resolution = 1;
        TweenerCore<Vector3, Path, PathOptions> t = DOTween
            .To(PathPlugin.Get(), () => target.position, (res) => target.MovePosition(res), new Path(pathType, path, resolution, gizmoColor), duration)
            .SetTarget(target)
            .SetUpdate(UpdateType.Fixed);

        t.plugOptions.isRigidbody = true;
        t.plugOptions.mode = pathMode;
        return t;
    }


    public static TweenerCore<Vector3, Path, PathOptions> DOPath(
           this Rigidbody2D target, Vector3[] path, float duration, PathType pathType = PathType.Linear,
           PathMode pathMode = PathMode.Full3D, int resolution = 10, Color? gizmoColor = null
       )
    {
        if (resolution < 1) resolution = 1;
        TweenerCore<Vector3, Path, PathOptions> t = DOTween
            .To(PathPlugin.Get(), () => target.position, (res) => target.MovePosition(res.ToVector2()), new Path(pathType, path, resolution, gizmoColor), duration)
            .SetTarget(target)
            .SetUpdate(UpdateType.Fixed);

        t.plugOptions.isRigidbody = true;
        t.plugOptions.mode = pathMode;
        return t;
    }

}
