using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace JoystickPlugin
{
    public enum JoystickType
    {
        Fixed = 0,
        Floating = 1,
        Dynamic = 2
    }


    public class VariableJoystick : Joystick
    {
        public UnityEvent onPressed { get; set; } = new UnityEvent();
        public UnityEvent onPressedUp { get; set; } = new UnityEvent();

        [SerializeField] private float moveThreshold = 1;
        [SerializeField] private JoystickType joystickType = JoystickType.Fixed;

        private Vector2 fixedPosition = Vector2.zero;

        public float MoveThreshold
        {
            get => moveThreshold;
            set => moveThreshold = Mathf.Abs(value);
        }


        public bool IsFixed => joystickType == JoystickType.Fixed;


        public void SetMode(JoystickType _joystickType)
        {
            joystickType = _joystickType;

            if (IsFixed)
            {
                background.anchoredPosition = fixedPosition;
            }

            background.gameObject.SetActive(IsFixed);
        }


        protected override void Start()
        {
            base.Start();

            fixedPosition = background.anchoredPosition;
            SetMode(joystickType);
        }


        public override void OnPointerDown(PointerEventData eventData)
        {
            if (joystickType != JoystickType.Fixed)
            {
                background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
                background.gameObject.SetActive(true);
            }

            base.OnPointerDown(eventData);

            onPressed?.Invoke();
        }


        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsFixed)
            {
                background.gameObject.SetActive(false);
            }

            base.OnPointerUp(eventData);

            onPressedUp?.Invoke();
        }


        protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            if (joystickType == JoystickType.Dynamic && magnitude > moveThreshold)
            {
                Vector2 difference = normalised * (magnitude - moveThreshold) * radius;
                background.anchoredPosition += difference;
            }
            base.HandleInput(magnitude, normalised, radius, cam);
        }
    }
}
