using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;


namespace Drawmasters.Ui.Extensions
{
    [RequireComponent(typeof(RectTransform))]
    public class UILineRenderer : UIPrimitiveBase
    {
        #region Fields

        private enum SegmentType
        {
            Start,
            Middle,
            End,
            Full,
        }

        private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;

        private static Vector2 UV_TOP_LEFT;
        private static Vector2 UV_BOTTOM_LEFT;
        private static Vector2 UV_TOP_CENTER_LEFT;
        private static Vector2 UV_TOP_CENTER_RIGHT;
        private static Vector2 UV_BOTTOM_CENTER_LEFT;
        private static Vector2 UV_BOTTOM_CENTER_RIGHT;
        private static Vector2 UV_TOP_RIGHT;
        private static Vector2 UV_BOTTOM_RIGHT;

        private static Vector2[] startUvs;
        private static Vector2[] middleUvs;
        private static Vector2[] endUvs;
        private static Vector2[] fullUvs;

        [Tooltip("Only for editor check. Setup points from code if you need play mode drawing")]
        [SerializeField] private RectTransform[] editorCheckPoints = default;
        
        [SerializeField] private float lineThickness = 2;

        #endregion



        #region Properties

        public Vector2[] Points { get; set; }

        #endregion



        #region Methods

        public void SetupPoints(Vector2[] _points)
        {
            if (Points == _points)
            {
                return;
            }

            Points = _points;
            SetAllDirty();
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (Points != null && Points.Length > 0)
            {
                GeneratedUVs();
                vh.Clear();

                Vector2[] pointForDrawing = default;
#if UNITY_EDITOR
                pointForDrawing = Application.isPlaying ? Points : editorCheckPoints.Select(e => e.anchoredPosition).ToArray();
#else
               pointForDrawing = Points;
#endif
                PopulateMesh(vh, pointForDrawing);
            }
        }


        private void PopulateMesh(VertexHelper vh, Vector2[] pointsToDraw)
        {
            pointsToDraw = IncreaseResolution(pointsToDraw);

            float sizeX = 1.0f;
            float sizeY = 1.0f;

            var offsetX = -rectTransform.pivot.x * sizeX;
            var offsetY = -rectTransform.pivot.y * sizeY;

            // Generate the quads that make up the wide line
            List<UIVertex[]> segments = new List<UIVertex[]>();

            for (var i = 1; i < pointsToDraw.Length; i++)
            {
                Vector2 start = new Vector2(pointsToDraw[i - 1].x * sizeX + offsetX, pointsToDraw[i - 1].y * sizeY + offsetY);
                Vector2 end = new Vector2(pointsToDraw[i].x * sizeX + offsetX, pointsToDraw[i].y * sizeY + offsetY);

                segments.Add(CreateLineSegment(start, end, SegmentType.Middle));
            }

            // Add the line segments to the vertex helper, creating any joins as needed
            for (int i = 0; i < segments.Count; i++)
            {
                if (i < segments.Count - 1)
                {
                    var vec1 = segments[i][1].position - segments[i][2].position;
                    var vec2 = segments[i + 1][2].position - segments[i + 1][1].position;
                    var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                    // Positive sign means the line is turning in a 'clockwise' direction
                    var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                    // Calculate the miter point
                    var miterDistance = lineThickness / (2 * Mathf.Tan(angle / 2));
                    var miterPointA = segments[i][2].position - vec1.normalized * miterDistance * sign;
                    var miterPointB = segments[i][3].position + vec1.normalized * miterDistance * sign;

                    if (miterDistance < vec1.magnitude * 0.5f && miterDistance < vec2.magnitude * 0.5f &&
                        angle > MIN_BEVEL_NICE_JOIN)
                    {
                        if (sign < 0)
                        {
                            segments[i][2].position = miterPointA;
                            segments[i + 1][1].position = miterPointA;
                        }
                        else
                        {
                            segments[i][3].position = miterPointB;
                            segments[i + 1][0].position = miterPointB;
                        }
                    }

                    var join = new UIVertex[] { segments[i][2], segments[i][3], segments[i + 1][0], segments[i + 1][1] };
                    vh.AddUIVertexQuad(join);
                }

                vh.AddUIVertexQuad(segments[i]);
            }

            if (vh.currentVertCount > 64000)
            {
                Debug.LogError($"Can't draw UI mesh. Max Verticies size is 64000. Current mesh verticies count is [{vh.currentVertCount}]");
                vh.Clear();
            }
        }


        private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type, UIVertex[] previousVert = null)
        {
            Vector2 offset = new Vector2((start.y - end.y), end.x - start.x).normalized * lineThickness / 2;

            bool wasPreviousVert = previousVert != null;

            Vector2 v1 = wasPreviousVert ? new Vector2(previousVert[3].position.x, previousVert[3].position.y) : (start - offset);
            Vector2 v2 = wasPreviousVert ? new Vector2(previousVert[2].position.x, previousVert[2].position.y) : (start + offset);

            Vector2 v3 = end + offset;
            Vector2 v4 = end - offset;

            //Return the VDO with the correct uvs
            switch (type)
            {
                case SegmentType.Start:
                    return SetVbo(new[] { v1, v2, v3, v4 }, startUvs);
                case SegmentType.End:
                    return SetVbo(new[] { v1, v2, v3, v4 }, endUvs);
                case SegmentType.Full:
                    return SetVbo(new[] { v1, v2, v3, v4 }, fullUvs);
                default:
                    return SetVbo(new[] { v1, v2, v3, v4 }, middleUvs);
            }
        }


        protected override void GeneratedUVs()
        {
            bool isSpriteActive = activeSprite != null;
            Vector4 outer = default;
            Vector4 inner = default;

            if (isSpriteActive)
            {
                outer = DataUtility.GetOuterUV(activeSprite);
                inner = DataUtility.GetInnerUV(activeSprite);
            }

            UV_TOP_LEFT = isSpriteActive ? new Vector2(outer.x, outer.y) : Vector2.zero;
            UV_BOTTOM_LEFT = isSpriteActive ? new Vector2(outer.x, outer.w) : new Vector2(0, 1);

            UV_TOP_CENTER_LEFT = isSpriteActive ? new Vector2(inner.x, inner.y) : new Vector2(0.5f, 0);
            UV_TOP_CENTER_RIGHT = isSpriteActive ? new Vector2(inner.z, inner.y) : new Vector2(0.5f, 0);

            UV_BOTTOM_CENTER_LEFT = isSpriteActive ? new Vector2(inner.x, inner.w) : new Vector2(0.5f, 1);
            UV_BOTTOM_CENTER_RIGHT = isSpriteActive ? new Vector2(inner.z, inner.w) : new Vector2(0.5f, 1);

            UV_TOP_RIGHT = isSpriteActive ? new Vector2(outer.z, outer.y) : new Vector2(1, 0);
            UV_BOTTOM_RIGHT = isSpriteActive ? new Vector2(outer.z, outer.w) : Vector2.one;

            startUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER_LEFT, UV_TOP_CENTER_LEFT };
            middleUvs = new[] { UV_TOP_CENTER_LEFT, UV_BOTTOM_CENTER_LEFT, UV_BOTTOM_CENTER_RIGHT, UV_TOP_CENTER_RIGHT };
            endUvs = new[] { UV_TOP_CENTER_RIGHT, UV_BOTTOM_CENTER_RIGHT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
            fullUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
        }


        protected override void ResolutionToNativeSize(float distance)
        {
            if (UseNativeSize)
            {
                m_Resolution = distance / (activeSprite.rect.width / pixelsPerUnit);
                lineThickness = activeSprite.rect.height / pixelsPerUnit;
            }
        }

        #endregion
    }
}
