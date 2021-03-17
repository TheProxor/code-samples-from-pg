using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class CurvedLineRenderer
    {
        #region Fields

        private const int points = 40;
        private const int startLinearCheckIndex = 35;

        private readonly LineRenderer line;
        private readonly List<RopeSegment> linePoints;

        private Vector3[] linePositions;

        private int segmentsBetweenTwoPoints;
        private Vector3[] lineSegments;

        private Coroutine renderingCoroutine;

        #endregion



        #region Class lifecycle

        public CurvedLineRenderer(LineRenderer lineRenderer, List<RopeSegment> ropeSegments)
        {
            line = lineRenderer;
            linePoints = ropeSegments;
        }

        #endregion



        #region Methods

        public void StartRendering()
        {
            linePositions = new Vector3[linePoints.Count];

            segmentsBetweenTwoPoints = (linePoints.Count - 1) > 0 ? Mathf.RoundToInt(points / (linePoints.Count - 1)) : 0;

            lineSegments = new Vector3[segmentsBetweenTwoPoints * (linePositions.Length - 1) + 1];

            renderingCoroutine = MonoBehaviourLifecycle.PlayCoroutine(Rendering());
        }


        public void StopRendering()
        {
            MonoBehaviourLifecycle.StopPlayingCorotine(renderingCoroutine);
            renderingCoroutine = null;
            line.positionCount = 0;
        }


        private IEnumerator Rendering()
        {
            line.positionCount = 0;

            AnimationCurve curveX = new AnimationCurve();
            AnimationCurve curveY = new AnimationCurve();

            while (linePoints.Count != 0)
            {
                for (int i = 0; i < linePoints.Count; i++)
                {
                    if (linePoints[i] != null)
                    linePositions[i] = linePoints[i].transform.position;
                }

                Keyframe[] keysX = new Keyframe[linePositions.Length];
                Keyframe[] keysY = new Keyframe[linePositions.Length];

                for (int i = 0; i < linePositions.Length; i++)
                {
                    keysX[i] = new Keyframe(i, linePositions[i].x);
                    keysY[i] = new Keyframe(i, linePositions[i].y);
                }

                curveX.keys = keysX;
                curveY.keys = keysY;

                for (int i = 0; i < linePositions.Length; i++)
                {
                    curveX.SmoothTangents(i, 0);
                    curveY.SmoothTangents(i, 0);
                }

                for (int i = 0; i < linePositions.Length; i++)
                {
                    lineSegments[i * segmentsBetweenTwoPoints] = linePositions[i];

                    if (i < linePositions.Length - 1)
                    {
                        for (int s = 1; s < segmentsBetweenTwoPoints; s++)
                        {
                            float time = (float)s / segmentsBetweenTwoPoints + i;
                            Vector2 newSegment = new Vector2(curveX.Evaluate(time), curveY.Evaluate(time));

                            lineSegments[i * segmentsBetweenTwoPoints + s] = newSegment;
                        }
                    }
                }

                int wrongSegmentsCount = 0;
                for(int i = startLinearCheckIndex; i < lineSegments.Length - 1; i++)
                {
                    if (lineSegments[i + 1].y > lineSegments[i].y)
                    {
                        wrongSegmentsCount++;
                    }
                }

                int arraySize = Mathf.Max(lineSegments.Length - wrongSegmentsCount - 1, 5);

                line.positionCount = arraySize;
                line.SetPositions(lineSegments);

                yield return null;
            }
        }

        #endregion
    }
}
