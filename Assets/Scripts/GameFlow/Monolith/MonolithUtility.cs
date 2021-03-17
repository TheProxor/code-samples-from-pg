using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using Drawmasters.Geometry;
using Drawmasters.Levels;
using System;


namespace Drawmasters.Monolith
{
    public static class MonolithUtility
    {
        #region Helpers

        private class AngleAccordance
        {
            private float sourceAngle;
            private float correctAngle;

            public AngleAccordance(float _sourceAngle, float _correctAngle)
            {
                sourceAngle = _sourceAngle;
                correctAngle = _correctAngle;
            }


            public bool IsSame(float angle, out float clampedAngle)
            {
                float delta = Mathf.Abs(angle) - Mathf.Abs(sourceAngle);

                delta = Mathf.Abs(delta);

                bool isSame = delta < AnglesDifference;

                if (isSame)
                {
                    clampedAngle = correctAngle;
                }
                else
                {
                    clampedAngle = angle;
                }

                return isSame;
            }
        }

        #endregion




        #region Fields

        const float PointsDifference = 1.0f;

        const float AnglesDifference = 0.25f;

        static readonly AngleAccordance[] Angles = { new AngleAccordance(63.435f, 60f),
                                                     new AngleAccordance(26.565f, 30f),
                                                     new AngleAccordance(90f,     90f),
                                                     new AngleAccordance(116.565f, 120f),
                                                     new AngleAccordance(57.5288f, 60f),
                                                     new AngleAccordance(45f, 45f),
                                                     new AngleAccordance(135f, 135f)
                                                   };


        #endregion



        #region Methods

        public static (Vector3, Vector3) GetLeftAndRightNearPoint(LevelObjectMonolith monolith, Vector3 anotherObjectPosition)
        {
            if (monolith == null)
            {
                throw new NullReferenceException("Monolith level object is null");
            }
            float minDistance = float.MaxValue;

            Vector3 leftNearPointPosition = Vector3.zero;
            Vector3 rightNearPointPosition = Vector3.zero;

            for (int i = 0; i < monolith.Spline.GetPointCount(); i++)
            {
                Vector3 splineWorldPosition = monolith.transform.TransformPoint(monolith.Spline.GetPosition(i));

                int nextPointNext = i + 1 >= monolith.Spline.GetPointCount() ? 0 : i + 1;
                Vector3 nextSplineWorldPosition = monolith.transform.TransformPoint(monolith.Spline.GetPosition(nextPointNext));

                float distance = CommonUtility.CalculateDistanceFromPointToSegment(anotherObjectPosition, splineWorldPosition, nextSplineWorldPosition);

                if (distance < minDistance)
                {
                    leftNearPointPosition = splineWorldPosition;
                    rightNearPointPosition = nextSplineWorldPosition;


                    minDistance = distance;
                }
            }

            return (leftNearPointPosition, rightNearPointPosition);
        }


        private static bool RoundAngle(float angle, out float clampedAngle)
        {
            bool result = default;

            clampedAngle = default;

            foreach (var i in Angles)
            {
                result = i.IsSame(angle, out clampedAngle);

                if (result)
                {
                    break;
                }
            }

            return result;
        }

        public static List<Vector2> GetUniquePoints(List<PointData> pointData)
        {
            HashSet<int> indexes = new HashSet<int>();
            List<Vector2> result = new List<Vector2>();

            foreach (var data in pointData)
            {
                if (!indexes.Contains(data.pointIndex))
                {
                    indexes.Add(data.pointIndex);

                    result.Add(data.pointPosition);
                }
            }

            return result;
        }

        public static void UpdateCollider(Collider2D collider,
                                          List<Vector2> sourcePoints,
                                          bool needClose = false)
        {
            if (collider is PolygonCollider2D polygon)
            {
                UpdateCollider(polygon,
                               sourcePoints,
                               needClose);
            }
            else if (collider is EdgeCollider2D edge)
            {
                UpdateCollider(edge,
                               sourcePoints,
                               needClose);
            }
        }

        public static void UpdateCollider(PolygonCollider2D polyCollider,
                                          List<Vector2> sourcePoints,
                                          bool needClose = false)
        {
            if (polyCollider != null)
            {
                Vector2[] polygonPoints = VerifyPoints(sourcePoints, needClose);

                polyCollider.offset = Vector2.zero;
                polyCollider.points = polygonPoints;
            }
        }

        public static void UpdateCollider(PolygonCollider2D polyCollider, Spline spline, bool needClose = false)
        {
            if (polyCollider != null)
            {
                Vector2[] polygonPoints = VerifyPoints(spline, needClose);

                polyCollider.offset = Vector2.zero;
                polyCollider.points = polygonPoints;
            }
        }


        public static void UpdateCollider(EdgeCollider2D edgeCollider, Spline spline, bool needClose = false)
        {
            if (edgeCollider != null)
            {
                Vector2[] edgePoints = VerifyPoints(spline, needClose);

                edgeCollider.Reset();
                edgeCollider.offset = Vector2.zero;
                edgeCollider.points = edgePoints;
            }
        }

        public static void UpdateCollider(EdgeCollider2D edgeCollider,
                                          List<Vector2> sourcePoints,
                                          bool needClose = false)
        {
            if (edgeCollider != null)
            {
                Vector2[] edgePoints = VerifyPoints(sourcePoints, needClose);

                edgeCollider.Reset();
                edgeCollider.offset = Vector2.zero;
                edgeCollider.points = edgePoints;
            }
        }


        private static Vector2[] VerifyPoints(Spline spline, bool forEdge)
        {
            int pointsCount = spline.GetPointCount();

            List<Vector2> points = new List<Vector2>(pointsCount);
            for (int i = 0; i < pointsCount; i++)
            {
                points[i] = spline.GetPosition(i);
            }

            return VerifyPoints(points, forEdge);
        }


        private static Vector2[] VerifyPoints(List<Vector2> points, bool forEdge)
        {
            int pointsCount = points.Count;
            List<Vector2> result = new List<Vector2>();

            for (int i = 0; i < pointsCount; i++)
            {
                bool isRightPoint = true;

                Vector3 currentPoints = points[i];

                if (i > 0)
                {
                    Vector3 previousPoint = points[i - 1];
                    float distance = Vector2.Distance(currentPoints.ToVector2(),
                                                      previousPoint.ToVector2());

                    isRightPoint = (distance > PointsDifference);
                }

                if (isRightPoint)
                {
                    result.Add(currentPoints);
                }
            }

            if (forEdge)
            {
                result.Add(points.First());
            }

            return result.ToArray();
        }


        public static List<Vector2> GetSplineOffset(Spline workSpline)
        {
            int count = workSpline.GetPointCount();

            Sprite tileSprite = IngameData.Settings.monolith.conturMonolithSprite;
            float offset = tileSprite.texture.height / tileSprite.pixelsPerUnit;

            List<Vector2> points = new List<Vector2>(count);

            for (int i = 0; i < count; i++)
            {
                Vector2 v = workSpline.GetPosition(i);

                points.Add(v);
            }

            return MakePointsOffset(points, offset);
        }


        public static List<Vector2> GetSplineOffset(List<Vector2> sourcePoints, out float offset)
        {
            Sprite tileSprite = IngameData.Settings.monolith.conturMonolithSprite;
            offset = tileSprite.texture.height / tileSprite.pixelsPerUnit;

            return MakePointsOffset(sourcePoints, offset);
        }


        public static float CornerAngle(CornerFormType formType,
                                        float parentAngle,
                                        float cornerAngle)
        {
            float result = default;

            if (Mathf.Abs(cornerAngle - 90f) < 1f)
            {
                if (formType == CornerFormType.Outer)
                {
                    result = parentAngle - 90f;
                }
                else if (formType == CornerFormType.Inner)
                {
                    result = parentAngle;
                }
            }
            else if (Mathf.Abs(cornerAngle - 45f) < 1f)
            {
                if (formType == CornerFormType.Outer)
                {
                    result = parentAngle;
                }
                else if (formType == CornerFormType.Inner)
                {
                    result = parentAngle;
                }
            }
            else if (Mathf.Abs(cornerAngle - 60f) < 1f)
            {
                if (formType == CornerFormType.Outer)
                {
                    result = parentAngle;
                }
                else if (formType == CornerFormType.Inner)
                {
                    result = parentAngle + 90f;
                }
            }
            else if (Mathf.Abs(cornerAngle - 30f) < 1f)
            {
                if (formType == CornerFormType.Outer)
                {
                    result = parentAngle;
                }
                else
                {
                    result = parentAngle;
                }
            }
            else if (Mathf.Abs(cornerAngle - 120f) < 1f)
            {
                if (formType == CornerFormType.Outer)
                {
                    result = parentAngle;
                }
                else
                {
                    result = parentAngle;
                }
            }
            else if (Mathf.Abs(cornerAngle - 135f) < 1f)
            {
                result = parentAngle;
            }


            return result;
        }


        public static Vector2 CornerOffset(CornerInfo corner)
        {
            CornerFormType fromType = corner.FormType;
            Sprite cornerSprite = corner.CornerSprite;
            float parentAngle = corner.ParentAngle;
            float cornerAngle = corner.CornerAngle;

            float unitsPerPixels = 1f / cornerSprite.pixelsPerUnit;

            float width = cornerSprite.texture.width * unitsPerPixels * Mathf.Sqrt(2f) * 0.5f;

            float angle = parentAngle;


            if (Mathf.Abs(cornerAngle - 90f) < 1f)
            {
                if (fromType == CornerFormType.Outer)
                {
                    angle += 45f;
                }
                else if (fromType == CornerFormType.Inner)
                {
                    angle += 45 + 90f;
                }
            }
            else if (Mathf.Abs(cornerAngle - 45f) < 1f)
            {
                if (fromType == CornerFormType.Outer)
                {
                    angle += 45f;
                }
                else if (fromType == CornerFormType.Inner)
                {
                    angle += 45f + 90f;
                }
            }
            else if (Mathf.Abs(cornerAngle - 60) < 1f)
            {
                if (fromType == CornerFormType.Outer)
                {
                    angle += 45f;
                }
                else if (fromType == CornerFormType.Inner)
                {
                    angle += 45f + 90f;
                }
            }
            else if (Mathf.Abs(cornerAngle - 30f) < 1f)
            {
                if (fromType == CornerFormType.Outer)
                {
                    angle += 45f;
                }
                else if (fromType == CornerFormType.Inner)
                {
                    angle += 45f + 90f;
                }
            }
            else if (Mathf.Abs(cornerAngle - 120f) < 1f)
            {
                if (fromType == CornerFormType.Outer)
                {
                    width = cornerSprite.texture.width * unitsPerPixels * 0.5f;
                }
                else
                {
                    float textureWidth = unitsPerPixels * cornerSprite.texture.width - 1f;


                    angle += 90f;

                    Vector2 tileOffsetV = new Vector2
                    {
                        x = Mathf.Cos(angle * Mathf.Deg2Rad) * corner.TileOffset,
                        y = Mathf.Sin(angle * Mathf.Deg2Rad) * corner.TileOffset
                    };

                    angle += 90f;

                    Vector2 spriteOffsetV = new Vector2
                    {
                        x = Mathf.Cos(angle * Mathf.Deg2Rad) * textureWidth,
                        y = Mathf.Sin(angle * Mathf.Deg2Rad) * textureWidth
                    };

                    return tileOffsetV + spriteOffsetV;
                }
            }
            else if (Mathf.Abs(cornerAngle - 150f) < 1f)
            {
                width = cornerSprite.texture.width * unitsPerPixels * 0.5f;
            }
            else if (Mathf.Abs(cornerAngle - 135f) < 1f)
            {
                //angle += 45f;

                width = cornerSprite.texture.width * unitsPerPixels * 0.5f;
            }


            Vector2 offset = new Vector2
            {
                x = Mathf.Cos(angle * Mathf.Deg2Rad),
                y = Mathf.Sin(angle * Mathf.Deg2Rad)
            };

            offset *= width;

            return offset;
        }


        public static List<CornerInfo> GetCorners(List<Vector2> points,
                                                  List<Vector2> sourcePoints,
                                                  WeaponType weaponType,
                                                  float offset)
        {
            List<CornerInfo> result = new List<CornerInfo>();

            int count = points.Count;

            for (int i = 0; i < count; i++)
            {
                int prevIndex = (i + count - 1) % count;
                int index = i;
                int nextIndex = (i + 1) % count;

                Vector2 v0 = sourcePoints[prevIndex];
                Vector2 v1 = sourcePoints[index];
                Vector2 v2 = sourcePoints[nextIndex];

                Vector2 correctV = points[index];

                PointPair p0 = new PointPair(v0, v1);
                PointPair p1 = new PointPair(v1, v2);

                bool isCounterClockwise = MathUtility.IsCounterClockwise(p0, p1, out float angle);

                angle = Mathf.Abs(angle);

                if (!RoundAngle(angle, out angle))
                {
                    //Debug.Log("not rounded angle " + angle);

                    continue;
                }

                CornerFormType formType = (isCounterClockwise) ?
                                            CornerFormType.Inner :
                                            CornerFormType.Outer;

                CornerInfo cornerInfo = new CornerInfo(formType,
                                                        correctV,
                                                        MathUtility.PositiveAngle(p0),
                                                        angle,
                                                        weaponType,
                                                        offset);

                if (cornerInfo.CornerSprite == null)
                {
                    continue;
                }
                
                result.Add(cornerInfo);
            }

            return result;
        }


        private static List<Vector2> MakePointsOffset(List<Vector2> points, float offset)
        {
            List<Vector2> result = new List<Vector2>();

            int count = points.Count;

            for (int i = 0; i < count; i++)
            {
                int prevIndex = (i + count - 1) % count;
                int index = i;
                int nextIndex = (i + 1) % count;

                Vector2 v0 = points[prevIndex];
                Vector2 v1 = points[index];
                Vector2 v2 = points[nextIndex];


                Vector2 d0 = VectorsNormalOffset(v0, v1, offset);
                Vector2 d1 = VectorsNormalOffset(v1, v2, offset);

                PointPair p0 = new PointPair(v0 + d0, v1 + d0);
                PointPair p1 = new PointPair(v1 + d1, v2 + d1);

                IntersectState state = MathUtility.RaysIntersect(p0, p1, out Vector2 v);

                if (state == IntersectState.Intersect)
                {
                    result.Add(v);
                }
                else
                {
                    result.Add(v1 + d0);
                }
            }

            return result;
        }


        private static Vector2 VectorsNormalOffset(Vector2 v0,
                                                   Vector2 v1,
                                                   float magnitude)
        {
            float angle = MathUtility.PositiveAngle(v0, v1);
            float rotatedAngle = angle - 90f;

            Vector2 direction = new Vector2
            {
                x = Mathf.Cos(rotatedAngle * Mathf.Deg2Rad),
                y = Mathf.Sin(rotatedAngle * Mathf.Deg2Rad)
            };

            return direction * magnitude;
        }

        #endregion
    }
}
