using System.Collections.Generic;
using Drawmasters.Levels;
using UnityEngine;
using UnityEngine.U2D;


namespace Drawmasters.Monolith
{
    public static class MonolithRenderUtility
    {
        public static void RenderMonolith(List<PointData> pointsData,
                                          ref List<CornerGraphic> cornerObjects,
                                          Collider2D shapeCollider,
                                          Spline shapeSpline,
                                          Transform parentTransform,
                                          WeaponType weaponType,
                                          bool forEdge = false)
        {
            cornerObjects.ForEach(corner => Content.Management.DestroyCorner(corner));
            cornerObjects.Clear();

            List<Vector2> sourcePoints = MonolithUtility.GetUniquePoints(pointsData);

            MonolithUtility.UpdateCollider(shapeCollider, sourcePoints, forEdge);

            List<Vector2> offsetPoints = MonolithUtility.GetSplineOffset(sourcePoints, out float offset);

            shapeSpline.Clear();

            List<CornerInfo> corners = MonolithUtility.GetCorners(offsetPoints,
                                                                  sourcePoints,
                                                                  WeaponType.Sniper,
                                                                  offset);

            for (int i = 0; i < offsetPoints.Count; i++)
            {
                Vector2 v = offsetPoints[i].ToVector3();

                shapeSpline.InsertPointAt(i, v);
                shapeSpline.SetTangentMode(i, ShapeTangentMode.Linear);
            }

            for (int i = 0; i < corners.Count; i++)
            {
                CornerInfo corner = corners[i];
                CornerGraphic newCorner = Content.Management.CreateCorner(corner, parentTransform);
                cornerObjects.Add(newCorner);
            }
        }
    }
}
