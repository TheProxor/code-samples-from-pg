using System;
using Drawmasters.Levels.Objects;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class PathPointUI : MonoBehaviour
    {
        #region Fields

        public event Action<PathPointUI> OnRemovalRequest;

        [SerializeField] private Vector3UI positionInput = default;
        [SerializeField] private Vector3UI rotationInput = default;
        [SerializeField] private UnityEngine.UI.Button removePointButton = default;

        #endregion



        #region Properties

        public DynamicPlank.PathPoint PathPoint { get; private set; }

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            removePointButton.onClick.AddListener(() => OnRemovalRequest?.Invoke(this));
            positionInput.OnValueChange += (position) => PathPoint.position = position;
            rotationInput.OnValueChange += (rotation) => PathPoint.rotation = rotation;
        }

        #endregion



        #region Methods

        public void Init(DynamicPlank.PathPoint pathPoint, int index)
        {
            PathPoint = pathPoint;

            positionInput.Init("Position " + index, 2);
            positionInput.SetCurrentValue(pathPoint.position);

            rotationInput.Init("Rotation " + index, 2);
            rotationInput.SetCurrentValue(pathPoint.rotation);
        }

        #endregion
    }
}
