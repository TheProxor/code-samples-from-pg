using UnityEngine;


namespace Drawmasters.Ui
{
    [RequireComponent(typeof(RectTransform))]
    public class IphoneXOffset : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Vector2 offset = default;

        private RectTransform targetTransform;

        private RectTransform TargetTransform
        {
            get
            {
                if (targetTransform == null)
                {
                    targetTransform = GetComponent<RectTransform>();
                }

                return targetTransform;
            }
        }

        #endregion



        #region Methods

        public void ChangeOffset()
        {
            if (ResolutionUtility.IsMonobrowDevice)
            {
                Vector3 position = TargetTransform.localPosition;

                position.x += offset.x;
                position.y += offset.y;

                TargetTransform.localPosition = position;
            }
        }

        #endregion
    }
}

