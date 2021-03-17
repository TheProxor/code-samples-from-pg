using System;
using DG.Tweening;
using UnityEngine;
using Drawmasters.Ua;
using ShakeData = Drawmasters.Levels.CameraShakeSettings.Shake;
using Drawmasters.CameraUtil;


namespace Drawmasters.Levels
{
    public class IngameCamera : SingletonMonoBehaviour<IngameCamera>, IInitializable, IDeinitializable
    {
        #region Helpers

        public class OffsetData
        {
            public float Aspec { get; private set; }

            public float Offset { get; private set; }

            public OffsetData(float aspect, float offset)
            {
                Aspec = aspect;
                Offset = offset;
            }
        }

        #endregion



        #region Fields

        private readonly Vector3 defaultCameraLocalPosition = new Vector3(0f, 0f, -100f);

        [SerializeField] private Camera ingameCamera = default;

        [SerializeField] private Transform movingRoot = default;

        private object zoomId;
        private object moveId;
        private object shakeId;

        private float defaultSize;
        private Vector3 defaultWorldPosition;

        StateComponent stateComponent;

        #endregion



        #region Properties

        public static bool IsSizeLocked { get; set; }

        public Camera Camera => ingameCamera;

        public IAction ScaleIn { get; private set; }

        public IAction ScaleOut { get; private set; }

        public static bool AllowShake { get; set; } = !BuildInfo.IsUaBuild;

        public static bool IsPortrait { get; set; } = true;

        #endregion



        #region IInitializable

        public void Initialize()
        {
            zoomId = Guid.NewGuid();
            moveId = Guid.NewGuid();
            shakeId = Guid.NewGuid();

            RefreshCameraSize();

            defaultWorldPosition = movingRoot.position;

            stateComponent = stateComponent ?? new StateComponent(Camera, defaultSize);
            stateComponent.Initialize();
        }

        #endregion



        #region IDenitializable

        public void Deinitialize()
        {
            stateComponent.Deinitialize();

            DOTween.Kill(zoomId);
            DOTween.Kill(moveId);
            DOTween.Kill(shakeId);
        }

        #endregion



        #region Methods        

        public void ClearAll()
        {
            DOTween.Kill(zoomId, true);
            DOTween.Kill(moveId, true);
            DOTween.Kill(shakeId, true);

            SetCameraSize(defaultSize);
            Camera.transform.localPosition = defaultCameraLocalPosition;

            movingRoot.position = defaultWorldPosition;
        }


        public void Zoom(float zoomFactor,
                         float duration,
                         AnimationCurve curve)
        {
            DOTween.Kill(zoomId, true);

            float zoomEndValue = Camera.orthographicSize * zoomFactor;

            Tween tween = Camera.DOOrthoSize(zoomEndValue, duration)
                            .SetId(zoomId);

            if (curve != null)
            {
                tween.SetEase(curve);
            }

            tween.Play();
        }


        public void MoveLocal(Vector3 local,
                              float duration,
                              AnimationCurve curve)
        {
            DOTween.Kill(moveId, true);

            Tween tween = movingRoot.DOLocalMove(local, duration)
                            .SetId(moveId);

            if (curve != null)
            {
                tween.SetEase(curve);
            }

            tween.Play();
        }


        public void Shake(ShakeData data, bool resetPreviousShake = true) => Shake(data.delay,
                                                   data.duration,
                                                   data.strength,
                                                   data.vibrato,
                                                   data.randomness,
                                                   resetPreviousShake);

        public void Shake(float delay,
                          float duration,
                          float strength,
                          int vibrato,
                          float randomness,
                          bool resetPreviousShake = true)
        {
            if (AllowShake)
            {
                if (resetPreviousShake)
                {
                    DOTween.Kill(shakeId, true);
                }

                if (!Mathf.Approximately(duration, 0.0f))
                {
                    Camera.DOShakePosition(duration, strength, vibrato, randomness, true)
                        .SetId(shakeId)
                        .SetDelay(delay);
                }
            }
        }


        public void RefreshCameraSize()
        {
            if (IsSizeLocked)
            {
                return;
            }

            float value = IsPortrait ?
                IngameData.Settings.ingameCameraSettings.portraitCameraSize :
                IngameData.Settings.ingameCameraSettings.landscapeCameraSize;

            SetCameraSize(value);
        }
        
     
        public void MoveLocalOffSetY(float offset, float duration, AnimationCurve curve)
        {
            Vector3 localPosition = movingRoot.localPosition;
            localPosition = new Vector3(localPosition.x, offset, localPosition.z);
            MoveLocal(localPosition, duration, curve);
        }
        

        private void SetCameraSize(float value)
        {
            if (IsSizeLocked)
            {
                return;
            }

            Camera.orthographicSize = value;
            defaultSize = Camera.orthographicSize;
        }

        #endregion
    }
}

