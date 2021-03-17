using System;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class GridObject : MonoBehaviour
    {
        #region Fields

        public static event Action<GridObject, bool> OnObjectActivityChange;

        private const float SizeDecreasingCoefficientForVisualizeLines = 0.2f;

        [SerializeField] private SpriteRenderer spriteRenderer = default;
        [SerializeField] private Color defaultObjectColor = default;
        [SerializeField] private Color objectColorUnderMouse = default;

        private Camera gridCamera;
        
        private bool isActive = true;
        private Rect objectRect = Rect.zero;

        #endregion



        #region Properties

        private bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    
                    ChangeObjectActivity(isActive);
                    
                    OnObjectActivityChange?.Invoke(this, isActive);
                }
            }
        }

        #endregion



        #region Unity lifecycle

        private void OnEnable()
        {
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }
        
        
        private void OnDisable()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }

        #endregion



        #region Methods

        public void Initialize(Camera screenCamera, Vector3 objectPosition, float objectSize)
        {
            gridCamera = screenCamera;
            transform.position = objectPosition;
            spriteRenderer.size = objectSize * SizeDecreasingCoefficientForVisualizeLines * Vector2.one;

            objectRect = new Rect(objectPosition.ToVector2() - objectSize * 0.5f * Vector2.one, objectSize * Vector2.one);

            IsActive = false;
        }


        private void ChangeObjectActivity(bool isObjectActive)
        {
            spriteRenderer.color = (isObjectActive) ? objectColorUnderMouse : defaultObjectColor;
        }


        private bool IsObjectContainsPoint(Vector3 point)
        {
            return objectRect.Contains(point);
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            IsActive = IsObjectContainsPoint(gridCamera.ScreenToWorldPoint(Input.mousePosition));
        }

        #endregion
    }
}

