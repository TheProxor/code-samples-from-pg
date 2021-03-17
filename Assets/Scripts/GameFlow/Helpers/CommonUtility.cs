using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Drawmasters
{
    public static class CommonUtility
    {
        #region Variables

        public const float OneFrameDelay = 0.0f;

        public static int[] DEFAULT_TRIANGLE_INDEXES = { 0, 1, 2 };

        #endregion



        #region Public methods


        public static Rect GetWorldRect(RectTransform rt, Vector2 scale)
        {
            // Convert the rectangle to world corners and grab the top left
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector3 topLeft = corners[0];
            // Rescale the size appropriately based on the current Canvas scale
            Vector2 scaledSize = new Vector2(scale.x * rt.rect.size.x, scale.y * rt.rect.size.y);

            return new Rect(topLeft, scaledSize);
        }


        public static Rect CalculateGameZoneRect(Camera camera, float gameZoneSizeMultiplier = 1.0f)
        {
            Camera gameCamera = camera;
            float cameraHalfWidth = gameCamera.orthographicSize * gameCamera.aspect * gameZoneSizeMultiplier;
            float cameraHalfHeight = gameCamera.orthographicSize * gameZoneSizeMultiplier;

            Vector2 cameraLowAnchor = new Vector2(gameCamera.transform.position.x - cameraHalfWidth, gameCamera.transform.position.y - cameraHalfHeight);
            Vector2 cameraSize = new Vector2(cameraHalfWidth * 2.0f, cameraHalfHeight * 2.0f);
            return new Rect(cameraLowAnchor, cameraSize);
        }


        public static Rect CalculateRect(Vector3 center, float width, float height)
        {
            float cameraHalfWidth = width * 0.5f;
            float cameraHalfHeight = height * 0.5f;

            Vector2 cameraLowAnchor = new Vector2(center.x - cameraHalfWidth, center.y - cameraHalfHeight);
            Vector2 rectSize = new Vector2(cameraHalfWidth, cameraHalfHeight);

            Rect result = new Rect(cameraLowAnchor, rectSize);
            DrawRect(result, Color.red);

            return result;
        }


        public static Vector2[] ToVector2Array(this Vector3[] array) =>
            Array.ConvertAll(array, (v) => v.ToVector2());


        public static Vector3[] ToVector3Array(this Vector2[] array) =>
            Array.ConvertAll(array, (v) => v.ToVector3());
        

        public static void Clear(this Array array)
        {
            if (array == null)
            {
                return;
            }

            Array.Clear(array, 0, array.Length);
        }


        public static T[] Add<T>(this T[] target, T item)
        {
            if (target == null)
            {
                return null;
            }

            T[] result = new T[target.Length + 1];
            target.CopyTo(result, 0);
            result[target.Length] = item;
            return result;
        }


        public static T[] AddRange<T>(this T[] target, T[] items)
        {
            if (target == null || items == null)
            {
                return null;
            }

            List<T> list = target.ToList();
            list.AddRange(items);

            return list.ToArray();
        }


        public static T[] Put<T>(this T[] target, T item, Predicate<T> match)
        {
            int foundSkinIndex = Array.FindIndex(target, match);

            if (foundSkinIndex != -1)
            {
                target[foundSkinIndex] = item;
            }
            else
            {
                target = target.Add(item);
            }

            return target;
        }


        public static bool IsIndexCorrect(this IList target, int i) =>
            i >= 0 && i < target.Count;


        public static T SafeGet<T>(this IList<T> target, int i)
        {
            if (i < 0 || i >= target.Count)
            {
                CustomDebug.Log("Wrong index access");
                return default;
            }

            return target[i];
        }

        public static bool SolveQuadraticEquation(float a, float b, float c, out float minRoot, out float maxRoot)
        {
            bool result = false;
            minRoot = -1f;
            maxRoot = -1f;

            if (Mathf.Approximately(a, 0f))
            {
                if (!Mathf.Approximately(b, 0f))
                {
                    result = true;

                    minRoot = -c / b;
                    maxRoot = minRoot;
                }
            }
            else
            {
                float discriminant = b * b - 4 * a * c;
                if (discriminant >= 0f)
                {
                    result = true;
                    discriminant = Mathf.Sqrt(discriminant);

                    float firstRoot = (-b - discriminant) * 0.5f / a;
                    float secondRoot = (-b + discriminant) * 0.5f / a;

                    if (firstRoot > secondRoot)
                    {
                        maxRoot = firstRoot;
                        minRoot = secondRoot;
                    }
                    else
                    {
                        maxRoot = secondRoot;
                        minRoot = firstRoot;
                    }
                }
            }

            return result;
        }


        public static void Clear(this StringBuilder sb)
        {
            if (sb.Length > 0)
            {
                sb.Remove(0, sb.Length);
            }
        }


        public static void SetObjectActive(GameObject go, bool active)
        {
            if (active != go.activeSelf)
            {
                go.SetActive(active);
            }
        }


        public static void SetObjectsActive(GameObject[] go, bool active)
        {
            if (go == null)
            {
                return;
            }

            foreach (var o in go)
            {
                if (active != o.activeSelf)
                {
                    o.SetActive(active);
                }
            }
        }


        public static IEnumerator CallInEndOfFrame(Action callback)
        {
            yield return new WaitForEndOfFrame();

            callback?.Invoke();
        }


        public static Vector2 Rotate(this Vector2 point, Vector2 anchor, float angle)
        {
            Vector2 result;
            point -= anchor;

            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
            float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            result.x = (cos * point.x) - (sin * point.y);
            result.y = (sin * point.x) + (cos * point.y);

            return result + anchor;
        }


        public static float NormalizeAngle(float angle)
        {
            if (angle < 0f || angle >= 360f)
            {
                angle -= ((int)(angle / 360f)) * 360f;

                if (angle < 0f)
                {
                    angle += 360f;
                }
            }

            return angle;
        }


        public static bool IsAngleBetweenAngles(float targetAngle, float minAngle, float maxAngle)
        {
            targetAngle = NormalizeAngle(targetAngle);
            minAngle = NormalizeAngle(minAngle);
            maxAngle = NormalizeAngle(maxAngle);

            if (maxAngle < minAngle)
            {
                maxAngle += 360f;
            }
            if (targetAngle < minAngle)
            {
                targetAngle += 360f;
            }

            return (minAngle <= targetAngle && targetAngle <= maxAngle);
        }


        public static float PositiveAngleDegree(Vector2 v1, Vector2 v2) =>
            PositiveAngleDegree(v2 - v1);


        public static float PositiveAngleDegree(Vector2 v)
        {
            float result = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

            if (result < 0f)
            {
                result += 360f;
            }

            return result;
        }


        public static bool IsDirectedOnAnotherVector(Vector2 vec1, Vector2 vec2)
        {
            Vector2 vec1Rotated90 = new Vector2(-vec1.y, vec1.x);
            float sign = (Vector2.Dot(vec1Rotated90, vec2) < 0) ? -1.0f : 1.0f;
            return sign < 0.0f;
        }


        public static bool IsLeftPoint(Vector2 targetPoint, Vector2 firstPoint, Vector2 secondPoint) // from given vector to vector with start point of out v and end with target point
        {
            Vector2 v = secondPoint - firstPoint;
            Vector2 vt = targetPoint - firstPoint;

            float va = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            float vta = Mathf.Atan2(vt.y, vt.x) * Mathf.Rad2Deg;

            float angle = vta - va;

            if (angle < 0f)
            {
                angle += 360f;
            }

            bool isConvex = (angle <= 180f);

            return isConvex;
        }





        public static T Find<T>(this T[] array, Func<T, bool> predicate)
        {
            T result = default(T);
            for (int i = 0, n = array.Length; i < n; i++)
            {
                if (predicate(array[i]))
                {
                    result = array[i];
                    break;
                }
            }

            return result;
        }


        public static bool Contains<T>(this T[] array, Func<T, bool> predicate)
        {
            bool result = false;
            for (int i = 0, n = array.Length; i < n; i++)
            {
                if (predicate(array[i]))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }


        public static bool Contains<T>(this List<T> list, Func<T, bool> predicate)
        {
            bool result = false;
            for (int i = 0, n = list.Count; i < n; i++)
            {
                if (predicate(list[i]))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }


        public static bool Same(this Vector3 thisVector, Vector2 vec2, float precision = 0.05f)
        {
            return Vector2.SqrMagnitude((Vector2)thisVector - vec2) <= precision * precision;
        }


        public static float RoundToValue(float sourceValue, float roundedValue)
        {
            return Mathf.Ceil(sourceValue / roundedValue) * roundedValue;
        }


        public static float ColliderOffset(Collider2D sourceCollider)
        {
            return Mathf.Max(sourceCollider.bounds.size.x, sourceCollider.bounds.size.x);
        }

        public static Vector3 CircumCirclePoint(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 result = Vector3.zero;

            float dx1 = v1.x - v0.x; float dy1 = v1.y - v0.y;
            if (Mathf.Approximately(dy1, 0f))
            {
                dy1 = float.Epsilon;
            }
            float angle1 = Mathf.Atan2(dy1, dx1) * Mathf.Rad2Deg;

            float dx2 = v2.x - v0.x; float dy2 = v2.y - v0.y;

            if (Mathf.Approximately(dy2, 0f))
            {
                dy2 = float.Epsilon;
            }

            float angle2 = Mathf.Atan2(dy2, dx2) * Mathf.Rad2Deg;

            float angleBis1 = (angle1 + angle2) * 0.5f;
            float kBis1 = Mathf.Tan(angleBis1 * Mathf.Deg2Rad);
            float yBis1 = v0.y - kBis1 * v0.x;


            float dx3 = v1.x - v2.x;
            float dy3 = v1.y - v2.y;

            if (Mathf.Approximately(dy3, 0f))
            {
                dy3 = float.Epsilon;
            }

            float angle3 = Mathf.Atan2(dy3, dx3) * Mathf.Rad2Deg;
            float angle3Bis = (angle1 + angle3) * 0.5f;
            float kBis3 = Mathf.Tan(angle3Bis * Mathf.Deg2Rad);
            float yBis3 = v1.y - kBis3 * v1.x;

            float x = (yBis3 - yBis1) / (kBis1 - kBis3);
            float y = kBis1 * x + yBis1;

            result = new Vector3(x, y, v0.z);

            return result;
        }


        public static string ToTotalMMSS(this TimeSpan timeSpan, string mask = "{0:D2}:{1:D2}")
        {
            string result = timeSpan.TotalSeconds > 0 ?
                string.Format(mask, (int)timeSpan.TotalMinutes, timeSpan.Seconds) :
                string.Format(mask, 0, 0);

            return result;
        }


        public static string ToTotalHHMMSS(this TimeSpan timeSpan, string mask = "{0:D2}:{1:D2}:{2:D2}")
        {
            string result = timeSpan.TotalSeconds > 0 ?
                string.Format(mask, (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds) :
                string.Format(mask, 0, 0, 0);

            return result;
        }


        public static string ToTotalHHMM(this TimeSpan timeSpan, string mask = "{0:D2}:{1:D2}")
        {
            string result = timeSpan.TotalSeconds > 0 ?
                string.Format(mask, (int)timeSpan.TotalHours, timeSpan.Minutes) :
                string.Format(mask, 0, 0);

            return result;
        }

        public static string ToTotalDDHHMM(this TimeSpan timeSpan, string mask = "{0:D2}d {1:D2}h {2:D2}m")
        {
            string result = timeSpan.TotalSeconds > 0 ?
                string.Format(mask, (int)timeSpan.TotalDays, (int)timeSpan.TotalHours, timeSpan.Minutes) :
                string.Format(mask, 0, 0, 0);

            return result;
        }


        public static void DrawRect(Rect rect, Color? color = null, float duration = 1.0f)
        {
            Vector2 bottomPointMin = new Vector2(rect.xMin, rect.yMin);
            Vector2 bottomPointMax = new Vector2(rect.xMax, rect.yMin);
            Vector2 topPointMin = new Vector2(rect.xMin, rect.yMax);
            Vector2 topPointMax = new Vector2(rect.xMax, rect.yMax);

            color = color ?? Color.black;
            Debug.DrawLine(bottomPointMin, bottomPointMax, color.Value, duration);
            Debug.DrawLine(topPointMin, topPointMax, color.Value, duration);
            Debug.DrawLine(bottomPointMin, topPointMin, color.Value, duration);
            Debug.DrawLine(bottomPointMax, topPointMax, color.Value, duration);
        }


        public static void DrawCircle(Vector3 centerPostition, float radius, int segmentsCount, Color color, bool shouldDrawRadius = true, float duration = -1f)
        {
            Vector3[] result = new Vector3[segmentsCount];
            float radianPerSegment = Mathf.Deg2Rad * 360f / segmentsCount;

            for (int i = 0; i < segmentsCount; i++)
            {
                float rad = i * radianPerSegment;
                result[i] = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius);
            }

            for (int i = 0; i < segmentsCount; i++)
            {
                if (duration <= 0f)
                {
                    Debug.DrawLine(centerPostition + result[i], centerPostition + result[(i + 1) % segmentsCount], color);
                }
                else
                {
                    Debug.DrawLine(centerPostition + result[i], centerPostition + result[(i + 1) % segmentsCount], color, duration);
                }
            }

            if (shouldDrawRadius)
            {
                if (duration <= 0f)
                {
                    Debug.DrawLine(centerPostition, centerPostition + result[0], color);
                }
                else
                {
                    Debug.DrawLine(centerPostition, centerPostition + result[0], color, duration);
                }
            }
        }


        public static void DrawArrow(Vector3 from, Vector3 to, Color color, float arrowHeadAngle = 10f, float arrowHeadLength = 5f)
        {
            float angle = Vector2.SignedAngle(Vector2.left, to - from) - 45f;
            Vector2 direction1 = Quaternion.Euler(0f, 0f, angle + arrowHeadAngle) * Vector2.one;
            Vector2 direction2 = Quaternion.Euler(0f, 0f, angle - arrowHeadAngle) * Vector2.one;

            Debug.DrawLine(from, to, color);
            Debug.DrawRay(to, direction1 * arrowHeadLength, color);
            Debug.DrawRay(to, direction2 * arrowHeadLength, color);
        }


        public static void DrawCross(Vector3 position, float size, float duration, Color color)
        {
            Debug.DrawLine(position + new Vector3(-size, -size), position + new Vector3(size, size), color, duration);
            Debug.DrawLine(position + new Vector3(-size, size), position + new Vector3(size, -size), color, duration);
        }


        public static string HierarchyPath(GameObject gameObject)
        {
            StringBuilder result = new StringBuilder();

            if (gameObject != null)
            {
                Transform parent = gameObject.transform.parent;
                result.Append(gameObject.name);

                while (parent != null)
                {
                    result.Append($"<{parent.name}");
                    parent = parent.transform.parent;
                }
            }
            else
            {
                result.Append("null");
            }

            return result.ToString();
        }



        public static int FirstElementIndex(string path, int defaultValue = -1)
        {
            int result = 0;

            int from = path.IndexOf('[');
            int to = path.IndexOf(']');

            if (to > from)
            {
                result = path.Substring(from + 1, to - 1 - from).ParseInt(defaultValue);
            }

            return result;
        }


        public static int LastElementIndex(string path, int defaultValue = -1)
        {
            int result = 0;

            int from = path.LastIndexOf('[');
            int to = path.LastIndexOf(']');

            if (to > from)
            {
                result = path.Substring(from + 1, to - 1 - from).ParseInt(defaultValue);
            }

            return result;
        }


        public static void RemoveAllComponents<T>(this GameObject go) where T : MonoBehaviour
        {
            if (go != null)
            {
                T[] array = go.GetComponents<T>();

                for (int i = 0; i < array.Length; i++)
                {
                    GameObject.Destroy(array[i]);
                }
            }
        }


        public static void RemoveComponent<T>(this GameObject go) where T : MonoBehaviour
        {
            if (go != null)
            {
                T component = go.GetComponent<T>();

                if (component != null)
                {
                    GameObject.Destroy(component);
                }
            }
        }

        public static float Distance2D(Vector3 a, Vector3 b)
        {
            return Vector2.Distance(a, b);
        }


        public static Vector3 CalculateCentralPoint(MonoBehaviour[] monoBehaviours)
        {
            Vector3 result = Vector3.zero;

            if (monoBehaviours != null && monoBehaviours.Length > 0)
            {
                foreach (var child in monoBehaviours)
                {
                    result += child.transform.position;
                }
                result /= monoBehaviours.Length;
            }

            return result;
        }


        public static float CalculateDistanceFromPointToSegment(Vector2 point, Vector2 leftSegmentPoint, Vector2 rightSegmentPoint)
        {
            float dx = rightSegmentPoint.x - leftSegmentPoint.x;
            float dy = rightSegmentPoint.y - leftSegmentPoint.y;
            if (Mathf.Approximately(dx, 0.0f) && Mathf.Approximately(dy, 0.0f))
            {
                dx = point.x - leftSegmentPoint.x;
                dy = point.y - leftSegmentPoint.y;
                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            float t = ((point.x - leftSegmentPoint.x) * dx + (point.y - leftSegmentPoint.y) * dy) / (dx * dx + dy * dy);

            if (t < 0)
            {
                dx = point.x - leftSegmentPoint.x;
                dy = point.y - leftSegmentPoint.y;
            }
            else if (t > 1)
            {
                dx = point.x - rightSegmentPoint.x;
                dy = point.y - rightSegmentPoint.y;
            }
            else
            {
                Vector2 closest = new Vector2(leftSegmentPoint.x + t * dx, leftSegmentPoint.y + t * dy);
                dx = point.x - closest.x;
                dy = point.y - closest.y;
            }

            return Mathf.Sqrt(dx * dx + dy * dy);
        }


        public static Vector2 PointOnExtents(Vector2 extents, Vector2 directionVector)
        {
            Vector2 direction = directionVector.normalized;
            float y = extents.x * direction.y / direction.x;

            float pointOnBoundsX = Mathf.Abs(y) < extents.y ? extents.x : extents.y * direction.x / direction.y;
            float pointOnBoundsY = Mathf.Abs(y) < extents.y ? y : extents.y;

            return new Vector2(pointOnBoundsX, pointOnBoundsY);
        }


        public static Vector3 NearestPointOnRect(Rect rect, Vector3 point)
        {
            #if UNITY_EDITOR
                DrawRect(rect);
            #endif

            Vector2 bottomPointMin = new Vector2(rect.xMin, rect.yMin);
            Vector2 bottomPointMax = new Vector2(rect.xMax, rect.yMin);
            Vector2 topPointMin = new Vector2(rect.xMin, rect.yMax);
            Vector2 topPointMax = new Vector2(rect.xMax, rect.yMax);

            Vector3 bottomPoint = NearestPointOnSegment(bottomPointMin, bottomPointMax, point);
            Vector3 topPoint = NearestPointOnSegment(topPointMin, topPointMax, point);
            Vector3 leftPoint = NearestPointOnSegment(bottomPointMin, topPointMin, point);
            Vector3 rightPoint = NearestPointOnSegment(bottomPointMax, topPointMax, point);

            Vector3[] points = {
                bottomPoint,
                topPoint,
                leftPoint,
                rightPoint
            };

            float minDistance = Mathf.Min(points.Select((e) => Vector3.Distance(e, point)).ToArray());
            int foundIndex = Array.FindIndex(points, e => Vector3.Distance(e, point) == minDistance);

            if (foundIndex == -1)
            {
                CustomDebug.Log($"No closest point found for rect = {rect}. point = {point}");
                return point;
            }

            return points[foundIndex];
        }

        public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);
             
            if (d == 0.0f)
            {
                return false;
            }

            var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            {
                return false;
            }

            intersection.x = p1.x + u * (p2.x - p1.x);
            intersection.y = p1.y + u * (p2.y - p1.y);

            return true;
        }


        public static bool LineRectIntersectionByDirection(Vector3 linePoint1, Vector3 lineDirection, Rect rect, out Vector3 intersection)
        {
            Vector2 bottomPointMin = new Vector2(rect.xMin, rect.yMin);
            Vector2 topPointMax = new Vector2(rect.xMax, rect.yMax);

            float rectDiagonal = Vector2.Distance(bottomPointMin, topPointMax);
            Vector3 linePoint2 = linePoint1 + lineDirection.normalized * rectDiagonal;

            bool isIntersect = LineRectIntersection(linePoint1, linePoint2, rect, out Vector3 result);
            intersection = result;

            return isIntersect;
        }


        public static bool LineRectIntersection(Vector3 linePoint1, Vector3 linePoint2, Rect rect, out Vector3 intersection)
        {
#if UNITY_EDITOR
            DrawRect(rect);
#endif
            Vector2 bottomPointMin = new Vector2(rect.xMin, rect.yMin);
            Vector2 bottomPointMax = new Vector2(rect.xMax, rect.yMin);
            Vector2 topPointMin = new Vector2(rect.xMin, rect.yMax);
            Vector2 topPointMax = new Vector2(rect.xMax, rect.yMax);

            (Vector3, Vector3)[] lines = {
                (bottomPointMin, bottomPointMax),
                (bottomPointMin,topPointMin),
                (topPointMax, bottomPointMax),
                (topPointMax, topPointMin),
            };

            (Vector3, Vector3)[] allIntersectionsLines = Array.FindAll(lines, e => LineSegmentsIntersection(linePoint1, linePoint2, e.Item1, e.Item2, out Vector2 result));
            bool isIntersect = allIntersectionsLines.Length > 0;

            Vector2[] allIntersections = allIntersectionsLines.Select((e) =>
            {
                LineSegmentsIntersection(linePoint1, linePoint2, e.Item1, e.Item2, out Vector2 result);
                return result;
            }).ToArray();


            float minDistance = Mathf.Min(allIntersections.Select((e) => Vector3.Distance(e, linePoint1)).ToArray());
            Vector3 direction = (linePoint2 - linePoint1).normalized;
            intersection = linePoint1 + direction * minDistance;

            if (isIntersect)
            {
#if UNITY_EDITOR
                DrawArrow(linePoint1, intersection, Color.red);
#endif
            }

            return isIntersect;
        }


        // http://wiki.unity3d.com/index.php/3d_Math_functions
        public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //is coplanar, and not parrallel
            if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = linePoint1 + (lineVec1 * s);
                return true;
            }
            else
            {
                intersection = Vector3.zero;
                return false;
            }
        }


        // https://forum.unity.com/threads/how-do-i-find-the-closest-point-on-a-line.340058/
        public static Vector3 NearestPointOnSegment(Vector3 start, Vector3 end, Vector3 pnt)
        {
            var line = (end - start);
            var len = line.magnitude;
            line.Normalize();

            var v = pnt - start;
            var d = Vector3.Dot(v, line);
            d = Mathf.Clamp(d, 0f, len);
            return start + line * d;
        }

        // https://forum.unity.com/threads/left-right-test-function.31420/
        public static float AngleDir(Vector2 A, Vector2 B) =>
            -A.x * B.y + A.y * B.x;


        public static bool IsOutOfBounds(Transform checkTransform, Bounds checkBounds, bool shouldCheckZ)
        {
            Vector3 checkPosition = checkTransform.position;

            if (!shouldCheckZ)
            {
                checkPosition.z = checkBounds.center.z;
            }

            return !checkBounds.Contains(checkPosition); ;
        }


        public static int ToPercents(this float value) => (int)(value * 100f);


        public static string ToStringPercents(this float value)
        {
            int percents = value.ToPercents();

            return percents.ToString();
        }

        #endregion



        #region Editor
#if UNITY_EDITOR

        public static UnityEngine.Object[] FindAssets(Type findType)
        {
            List<UnityEngine.Object> result = new List<UnityEngine.Object>();

            string filter = $"t:{findType.Name}";
            string[] guids = AssetDatabase.FindAssets(filter);

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object unityObject = AssetDatabase.LoadAssetAtPath(path, findType);

                result.Add(unityObject);
            }

            return result.ToArray();
        }

#endif
        #endregion

    }
}
