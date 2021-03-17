using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Drawmasters.Helpers;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class TrajectoryDrawer : ShooterAimingDrawer
    {
        #region Fields

        private readonly Gradient shotRendererGradient;
        private readonly Gradient beginRendererGradient;
        private readonly Gradient endRendererGradient;

        private readonly TrajectoryDrawSettings drawSettings;

        private List<Vector3> trajectory = new List<Vector3>();

        private Coroutine stopSoundRoutine;

        private LineRenderer trajectoryRenderer;
        private LineRenderer beginTrajectoryRenderer;
        private LineRenderer endTrajectoryRenderer;

        #endregion



        #region Properties

        public override float PathDistance
        {
            get
            {
                float distance = 0f;

                for (int i = 0; i < trajectory.Count - 1; i++)
                {
                    Vector2 a = trajectory[i];
                    Vector2 b = trajectory[i + 1];

                    distance += Vector2.Distance(a, b);
                }

                return distance;
            }
        }

        public override Vector2[] CurrentProjectileTrajectory
        {
            get
            {
                int totalPositionsCount = beginTrajectoryRenderer.positionCount + trajectoryRenderer.positionCount + endTrajectoryRenderer.positionCount;
                List<Vector2> result = new List<Vector2>(totalPositionsCount);

                result.AddRange(CalculateTrajectory(beginTrajectoryRenderer));
                result.AddRange(CalculateTrajectory(trajectoryRenderer));
                result.AddRange(CalculateTrajectory(endTrajectoryRenderer));

                return result.ToArray();

                List<Vector2> CalculateTrajectory(LineRenderer lineRenderer)
                {
                    if (lineRenderer == null)
                    {
                        return new List<Vector2>();
                    }

                    List<Vector2> lineTrajectory = new List<Vector2>(lineRenderer.positionCount);

                    for (int i = 0; i < lineRenderer.positionCount; i++)
                    {
                        lineTrajectory.Add(lineRenderer.GetPosition(i));
                    }

                    return lineTrajectory;
                }
            }
        }

        public override Vector2 CurrentDirection { get; protected set; }

        protected override string UnderFingerFxKey { get; }

        protected override string DrawSfxKey { get; }

        public override Vector2 StartDirection
        {
            get
            {
                Vector2[] trajectory = CurrentProjectileTrajectory;
                return trajectory.Length > 1 ? trajectory[1] - trajectory[0] : Vector2.left;
            }
        }

        #endregion



        #region Class lifecycle

        public TrajectoryDrawer(ShooterColorType shooterColorType, TrajectoryDrawSettings _drawSettings)
        {
            drawSettings = _drawSettings;

            shotRendererGradient = drawSettings.FindDrawLineRendererGradient(shooterColorType);
            beginRendererGradient = drawSettings.FindPreviousLineRendererGradient(shooterColorType);
            endRendererGradient = GraphicsUtility.GetReversedGradient(beginRendererGradient);

            UnderFingerFxKey = drawSettings.FindUnderFingerFxKey(shooterColorType);
            DrawSfxKey = drawSettings.FindDrawSfxKey(shooterColorType);
        }

        #endregion



        #region Methods

        public override void Initialize(Transform levelTransform, WeaponType mode)
        {
            trajectoryRenderer = Content.Management.CreateShotLineRenderer(levelTransform);
            beginTrajectoryRenderer = Content.Management.CreateShotLineRenderer(levelTransform);
            endTrajectoryRenderer = Content.Management.CreateShotLineRenderer(levelTransform);

            base.Initialize(levelTransform, mode); // cuz firstly create renderer and then apply settings

            SetupRendererSettings(trajectoryRenderer, drawSettings.shotRendererWidth, drawSettings.shotRendererWidth, shotRendererGradient);
            SetupRendererSettings(beginTrajectoryRenderer, drawSettings.borderShotRendererWidthStart, drawSettings.borderShotRendererWidthFinish, beginRendererGradient);
            SetupRendererSettings(endTrajectoryRenderer, drawSettings.borderShotRendererWidthFinish, drawSettings.borderShotRendererWidthStart, endRendererGradient);

            SetupGeneralSettings(trajectoryRenderer, drawSettings);
            SetupGeneralSettings(beginTrajectoryRenderer, drawSettings);
            SetupGeneralSettings(endTrajectoryRenderer, drawSettings);

            void SetupRendererSettings(LineRenderer lineRenderer, float startWidth, float endWidth, Gradient gradient)
            {
                lineRenderer.startWidth = startWidth;
                lineRenderer.endWidth = endWidth;
                lineRenderer.colorGradient = gradient;
            }

            void SetupGeneralSettings(LineRenderer lineRenderer, TrajectoryDrawSettings targetDrawSettings)
            {
                lineRenderer.material = targetDrawSettings.material;
                lineRenderer.textureMode = targetDrawSettings.textureMode;
                lineRenderer.numCornerVertices = targetDrawSettings.cornerVertices;
            }
        }


        public override void Deinitialize()
        {
            base.Deinitialize();

            DestroyRenderer(ref trajectoryRenderer);
            DestroyRenderer(ref beginTrajectoryRenderer);
            DestroyRenderer(ref endTrajectoryRenderer);

            MonoBehaviourLifecycle.StopPlayingCorotine(stopSoundRoutine);

            void DestroyRenderer(ref LineRenderer curvedLineRenderer)
            {
                if (curvedLineRenderer != null)
                {
                    Content.Management.DestroyObject(curvedLineRenderer.gameObject);
                    curvedLineRenderer = null;
                }
            }

            DOTween.Kill(this);
        }


        public override void StartDrawing(Vector2 touchPosition)
        {
            base.StartDrawing(touchPosition);

            CurrentDirection = StartDirection;

            ClearTrajectoreLine(true);

            RefreshTrajectoryLineRenderer();
        }


        public override void DrawShotDirection(Vector2 startPosition, Vector2 touchPosition)
        {
            base.DrawShotDirection(startPosition, touchPosition);

            trajectory.Add(touchPosition);

            RefreshTrajectoryLineRenderer();
            CurrentDirection = (touchPosition - startPosition).normalized;

            PlaySound();

            MonoBehaviourLifecycle.StopPlayingCorotine(stopSoundRoutine);
            stopSoundRoutine = MonoBehaviourLifecycle.PlayCoroutine(StopSoundRoutine());
        }


        public override void EndDrawShotDirection(Vector2 startPosition, Vector2 touchPosition)
        {
            int index = trajectory.IndexOf(touchPosition);

            if (index == -1)
            {
                trajectory.Add(touchPosition);
            }
            else
            {
                trajectory = trajectory.Take(index + 1).ToList();
            }

            RefreshTrajectoryLineRenderer();
            CurrentDirection = (touchPosition - startPosition).normalized;

            base.EndDrawShotDirection(startPosition, touchPosition);
        }


        public override void ClearDraw(bool isImmediately)
        {
            base.ClearDraw(isImmediately);
            ClearTrajectoreLine(isImmediately);
        }


        protected override void ApplySettings(WeaponType type) { }


        public override void SetReloadVisualEnabled(bool value)
        {
            base.SetReloadVisualEnabled(value);

            SetRendererVisual(trajectoryRenderer, shotRendererGradient, value);
            SetRendererVisual(beginTrajectoryRenderer, beginRendererGradient, value);
            SetRendererVisual(endTrajectoryRenderer, endRendererGradient, value);

            void SetRendererVisual(LineRenderer lineRenderer, Gradient gradient, bool enabled)
            {
                if (lineRenderer != null)
                {
                    Gradient alphaGradient = enabled ? GraphicsUtility.GetAlphaGradient() : gradient;
                    lineRenderer.colorGradient = alphaGradient;
                }
            }
        }

        private void ClearTrajectoreLine(bool isImmediately)
        {
            DOTween.Kill(this, true);
            
            if (isImmediately)
            {
                ClearRenderer(trajectoryRenderer);
                ClearRenderer(beginTrajectoryRenderer);
                ClearRenderer(endTrajectoryRenderer);
                trajectory.Clear();
            }
            else
            {
                int allPositionsCount = beginTrajectoryRenderer.positionCount + trajectoryRenderer.positionCount +
                                        endTrajectoryRenderer.positionCount;

                Vector3[] beginPositions = new Vector3[beginTrajectoryRenderer.positionCount]; ;
                beginTrajectoryRenderer.GetPositions(beginPositions);
                Vector3[] trajectoryPositions = new Vector3[trajectoryRenderer.positionCount]; ;
                trajectoryRenderer.GetPositions(trajectoryPositions);
                Vector3[] endPositions = new Vector3[endTrajectoryRenderer.positionCount]; ;
                endTrajectoryRenderer.GetPositions(endPositions);
                List<Vector3> resultPositions = new List<Vector3>();

                resultPositions.AddRange(beginPositions);
                resultPositions.AddRange(trajectoryPositions);
                resultPositions.AddRange(endPositions);

                Color baseColor = default;
                foreach (var colorKey in trajectoryRenderer.colorGradient.colorKeys)
                {
                    if (colorKey.color != Color.black)
                    {
                        baseColor = colorKey.color;
                        break;
                    }
                }

                ClearRenderer(beginTrajectoryRenderer, endTrajectoryRenderer);

                trajectoryRenderer.positionCount = allPositionsCount;
                trajectoryRenderer.SetPositions(resultPositions.ToArray());

                float duration = CalculateDuration();

                DOTween.To(() => 0f, SetAnimationFactor, 1f, duration)
                    .SetEase(drawSettings.animationEraseLine.curve)
                    .SetUpdate(drawSettings.animationEraseLine.shouldUseUnscaledDeltaTime)
                    .OnComplete(() =>
                    {
                        ClearRenderer(beginTrajectoryRenderer, trajectoryRenderer, endTrajectoryRenderer);

                        trajectory.Clear();
                        trajectoryRenderer.colorGradient = shotRendererGradient;
                    })
                    .SetId(this);

                void SetAnimationFactor(float x)
                {
                    if (x < 0.94f)
                    {
                        trajectoryRenderer.colorGradient = GraphicsUtility.GetAlphaGradient(baseColor,
                            x, 0.2f);
                    }
                    else if (trajectoryRenderer.positionCount > 0)
                    {
                        ClearRenderer(trajectoryRenderer);
                    }
                }

                float CalculateDuration()
                {
                    if (clearDuration <= initalClearDuration)
                    {
                        float defaultDuration = drawSettings.animationEraseLine.duration * allPositionsCount / 500;
                        return defaultDuration;
                    }
                    else
                    {
                        return clearDuration;
                    }
                }
            }

            void ClearRenderer(params LineRenderer[] lineRenderers)
            {
                foreach (LineRenderer lineRenderer in lineRenderers)
                {
                    if (lineRenderer != null)
                    {
                        lineRenderer.SetPositions(Array.Empty<Vector3>());
                        lineRenderer.positionCount = 0;
                    }
                }
            }
        }


        private void RefreshTrajectoryLineRenderer()
        {
            Vector3[] smoothedPoints = LineSmoother.SmoothLine(trajectory.ToArray(), drawSettings.lineSegmentSize);

            Vector3[] mainTrajectoryPoints = smoothedPoints
                .Skip(drawSettings.beginSegmentsCount - drawSettings.borderRenderersOffsetSegmentsCount)
                .SkipLast(drawSettings.endSegmentsCount - drawSettings.borderRenderersOffsetSegmentsCount)
                .ToArray();
            trajectoryRenderer.SetVertices(mainTrajectoryPoints);

            Vector3[] beginTrajectoryPoints = smoothedPoints.TakeMax(drawSettings.beginSegmentsCount).ToArray();
            beginTrajectoryRenderer.SetVertices(beginTrajectoryPoints);

            Vector3[] endTrajectoryPoints = smoothedPoints.TakeLast(drawSettings.endSegmentsCount).ToArray();
            endTrajectoryRenderer.SetVertices(endTrajectoryPoints);
        }


        private IEnumerator StopSoundRoutine()
        {
            float delay = IngameData.Settings.trajectoryDrawSettings.stayDelaytoStopSound;
            yield return new WaitForSeconds(delay);

            StopSound();
        }

        #endregion
    }
}