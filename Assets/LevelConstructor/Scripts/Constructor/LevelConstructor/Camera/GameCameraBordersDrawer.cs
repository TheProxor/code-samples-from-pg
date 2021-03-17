using System;
using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class GameCameraBordersDrawer : MonoBehaviour
    {
        #region Fields

        [Header("Portait")]
        private const float Aspect16x9 = 9.0f / 16.0f;
        private const float Aspect4x3 = 3.0f / 4.0f;
        private const float Aspect21x9 = 9.0f / 21.0f;

        [Header("Landscape")]
        private const float Aspect9x16 = 16.0f / 9.0f;
        private const float Aspect3x4 = 4.0f / 3.0f;
        private const float Aspect9x21 = 21.0f / 9.0f;

        private const float UpperUiHeight = 23.4f;
        private const float LowerUiHeight = 19.7f;

        [SerializeField] private Camera gameCamera = default;

        [Header("Portait")]
        [SerializeField] private LineRenderer drawer16x9 = default;
        [SerializeField] private Color drawer16x9Color = default;
        [SerializeField] private LineRenderer drawer4x3 = default;
        [SerializeField] private Color drawer4x3Color = default;
        [SerializeField] private LineRenderer drawer21x9 = default;
        [SerializeField] private Color drawer21x9Color = default;

        [Header("Landscape")]
        [SerializeField] private LineRenderer drawer9x16 = default;
        [SerializeField] private LineRenderer drawer3x4 = default;
        [SerializeField] private LineRenderer drawer9x21 = default;

        [SerializeField] private LineRenderer upperUiDrawer = default;
        [SerializeField] private LineRenderer lowerUiDrawer = default;

        private static bool shouldDrawPortraitBorders = true; // cuz of two GameCameraBordersDrawer on diff scenes

        #endregion



        #region Properties

        public Camera GameCamera => gameCamera;


        public float LeftCameraMaxPositionX { get; private set; }


        public float RightCameraMaxPositionX { get; private set; }


        public float TopCameraMaxPositionY { get; private set; }


        public float BottomCameraMaxPositionY { get; private set; }

        #endregion



        #region Unity lifecycle

        private void Start()
        {
            drawer16x9.material.color = drawer16x9Color;
            drawer4x3.material.color = drawer4x3Color;
            drawer21x9.material.color = drawer21x9Color;

            drawer9x16.material.color = drawer16x9Color;
            drawer3x4.material.color = drawer4x3Color;
            drawer9x21.material.color = drawer21x9Color;

            upperUiDrawer.material.color = drawer16x9Color;
            lowerUiDrawer.material.color = drawer16x9Color;
            RefreshCameraSize();
        }


        private void Awake()
        {
            LevelEditor.OnShouldChangeCameraBorders += LevelEditor_OnShouldChangeCameraBorders;
        }


        private void OnDestroy()
        {
            LevelEditor.OnShouldChangeCameraBorders -= LevelEditor_OnShouldChangeCameraBorders;
        }


        private void Update()
        {
            if (shouldDrawPortraitBorders)
            {
                RecalculateBordersMainCamera();
            }
            else
            {
                RecalculateBordersMainCameraLandscape();
            }
        }

        #endregion



        #region Methods

        private void RecalculateBordersMainCamera()
        {
            Vector3 cameraPosition = gameCamera.transform.position;
            float defaultHeight = gameCamera.orthographicSize * 2.0f;
            float defaultWidth = defaultHeight * Aspect16x9;
            SetBorderPoints(cameraPosition, defaultWidth, defaultHeight, drawer16x9);

            UpdateUpperUiBounds(cameraPosition, defaultWidth, defaultHeight);
            UpdateLowerUiBounds(cameraPosition, defaultWidth, defaultHeight);

            float width21x9 = defaultHeight * Aspect21x9;
            SetBorderPoints(cameraPosition, width21x9, defaultHeight, drawer21x9);

            Vector3 cameraPosition4x3 = cameraPosition;
            float width4x3 = defaultHeight * Aspect4x3;

            SetBorderPoints(cameraPosition4x3, width4x3, defaultHeight, drawer4x3);
        }


        private void RecalculateBordersMainCameraLandscape()
        {
            Vector3 cameraPosition = gameCamera.transform.position;
            float defaultHeight = gameCamera.orthographicSize * 2.0f;

            float defaultWidth = defaultHeight * Aspect9x16;
            SetBorderPoints(cameraPosition, defaultWidth, defaultHeight, drawer9x16);

            UpdateUpperUiBounds(cameraPosition, defaultWidth, defaultHeight);
            UpdateLowerUiBounds(cameraPosition, defaultWidth, defaultHeight);

            float width9x21 = defaultHeight * Aspect9x21;
            SetBorderPoints(cameraPosition, width9x21, defaultHeight, drawer9x21);

            Vector3 cameraPosition3x4 = cameraPosition;
            float width3x4 = defaultHeight * Aspect3x4;

            SetBorderPoints(cameraPosition3x4, width3x4, defaultHeight, drawer3x4);
        }


        private void UpdateUpperUiBounds(Vector3 cameraPosition,
                                         float width,
                                         float height)
        {
            Vector3 center = cameraPosition.SetZ(0f);

            Vector3[] points = new Vector3[5];

            points[0] = center + new Vector3(-width * 0.5f, height * 0.5f - UpperUiHeight);
            points[1] = center + new Vector3(-width * 0.5f, height * 0.5f);
            points[2] = center + new Vector3(width * 0.5f, height * 0.5f);
            points[3] = center + new Vector3(width * 0.5f, height * 0.5f - UpperUiHeight);
            points[4] = points[0];

            upperUiDrawer.positionCount = points.Length;
            upperUiDrawer.SetPositions(points);
        }


        private void UpdateLowerUiBounds(Vector3 cameraPosition,
                                         float width,
                                         float height)
        {
            Vector3 center = cameraPosition.SetZ(0f);

            Vector3[] points = new Vector3[5];

            points[0] = center + new Vector3(-width * 0.5f, -height * 0.5f + UpperUiHeight);
            points[1] = center + new Vector3(-width * 0.5f, -height * 0.5f);
            points[2] = center + new Vector3(width * 0.5f, -height * 0.5f);
            points[3] = center + new Vector3(width * 0.5f, -height * 0.5f + UpperUiHeight);
            points[4] = points[0];

            lowerUiDrawer.positionCount = points.Length;
            lowerUiDrawer.SetPositions(points);
        }


        private void SetBorderPoints(Vector3 cameraPosition, float width, float height, LineRenderer drawer)
        {
            Vector3 zoneCenter = cameraPosition.SetZ(0);

            Vector3[] points = new Vector3[5];

            points[0] = zoneCenter + new Vector3(-width / 2, height / 2);
            points[1] = zoneCenter + new Vector3(width / 2, height / 2);
            points[2] = zoneCenter + new Vector3(width / 2, -height / 2);
            points[3] = zoneCenter + new Vector3(-width / 2, -height / 2);
            points[4] = points[0];

            drawer.positionCount = 5;
            drawer.SetPositions(points);

            UpdateCameraMaxBordersPositions(points);
        }


        private void UpdateCameraMaxBordersPositions(Vector3[] points)
        {
            foreach (Vector3 point in points)
            {
                if (LeftCameraMaxPositionX > point.x)
                {
                    LeftCameraMaxPositionX = point.x;
                }
                else if (RightCameraMaxPositionX < point.x)
                {
                    RightCameraMaxPositionX = point.x;
                }

                if (BottomCameraMaxPositionY > point.y)
                {
                    BottomCameraMaxPositionY = point.y;
                }
                else if (TopCameraMaxPositionY < point.y)
                {
                    TopCameraMaxPositionY = point.y;
                }
            }
        }

        #endregion



        #region Events handlers

        private void LevelEditor_OnShouldChangeCameraBorders()
        {
            shouldDrawPortraitBorders = !shouldDrawPortraitBorders;
            RefreshCameraSize();

            if (shouldDrawPortraitBorders)
            {
                drawer9x16.positionCount = 0;
                drawer3x4.positionCount = 0;
                drawer9x21.positionCount = 0;

                drawer9x16.SetPositions(Array.Empty<Vector3>());
                drawer3x4.SetPositions(Array.Empty<Vector3>());
                drawer9x21.SetPositions(Array.Empty<Vector3>());

            }
            else
            {
                drawer16x9.positionCount = 0;
                drawer4x3.positionCount = 0;
                drawer21x9.positionCount = 0;

                drawer16x9.SetPositions(Array.Empty<Vector3>());
                drawer4x3.SetPositions(Array.Empty<Vector3>());
                drawer21x9.SetPositions(Array.Empty<Vector3>());
            }
        }

        private void RefreshCameraSize()
        {
            float value = shouldDrawPortraitBorders ?
                IngameData.Settings.ingameCameraSettings.portraitCameraSize :
                IngameData.Settings.ingameCameraSettings.landscapeCameraSize;
            gameCamera.orthographicSize = value;

            IngameCamera.IsPortrait = shouldDrawPortraitBorders;
        }

        #endregion

    }
}
