using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Drawmasters.LevelConstructor
{
    public class RopeInspectorExtension : InspectorExtensionBase
    {
        #region Fields

        [SerializeField] private FloatInputUi lengthChangeInput = default;
        [SerializeField] private FloatInputUi segmentsShiftChangeInput = default;
        [SerializeField] private TMP_Text actualLengthText = default;

        [SerializeField] private Button endPositionToCenterButton = default;
        [SerializeField] private DraggableObject endPositionPrefab = default;
        [SerializeField] private BoolInputUi sphericalTrajectory = default;

        private EditorRope editorObject;
        private DraggableObject hookConnectedDraggableObject;
        private DraggableObject endConnectedDraggableObject;

        #endregion



        #region Methods

        public override void Init(EditorLevelObject levelObject)
        {
            editorObject = levelObject as EditorRope;

            if (editorObject != null)
            {
                lengthChangeInput.Init("Length", editorObject.Length, 1.0f);
                segmentsShiftChangeInput.Init("Segments shift", editorObject.SegmentsShift, 1.0f);
            }

            sphericalTrajectory.Init("Is Spherical Trajectory", editorObject.IsSphericalTrajectory);

            InitializeDraggableObject(hookConnectedDraggableObject, editorObject.HookConnectedObjectPosition - editorObject.HookAnchorOffset, Color.green);
            InitializeDraggableObject(endConnectedDraggableObject, editorObject.ConnectedObjectPosition - editorObject.EndAnchorOffset, Color.blue);
        }


        protected override void SubscribeOnEvents()
        {
            lengthChangeInput.OnValueChange += LengthChangeInput_OnValueChange;
            segmentsShiftChangeInput.OnValueChange += SegmentsShiftChangeInput_OnValueChange;
            endPositionToCenterButton.onClick.AddListener(EndPositionToCenterButton_OnClick);

            sphericalTrajectory.OnValueChange += SphericalTrajectory_OnValueChange;

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            endConnectedDraggableObject = Instantiate(endPositionPrefab);
            hookConnectedDraggableObject = Instantiate(endPositionPrefab);

            SubscribeOnDraggableObject(endConnectedDraggableObject);
            SubscribeOnDraggableObject(hookConnectedDraggableObject);
        }



        protected override void UnsubscribeFromEvents()
        {
            lengthChangeInput.OnValueChange -= LengthChangeInput_OnValueChange;
            segmentsShiftChangeInput.OnValueChange -= SegmentsShiftChangeInput_OnValueChange;
            endPositionToCenterButton.onClick.RemoveListener(EndPositionToCenterButton_OnClick);

            sphericalTrajectory.OnValueChange -= SphericalTrajectory_OnValueChange;

            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            UnsubscribeOnDraggableObject(endConnectedDraggableObject);
            UnsubscribeOnDraggableObject(hookConnectedDraggableObject);
        }


        private void InitializeDraggableObject(DraggableObject draggableObject, Vector3 position, Color color)
        {
            if (draggableObject != null)
            {
                draggableObject.transform.position = position;
                draggableObject.GetComponent<SpriteRenderer>().color = color;
            }
        }


        private void SubscribeOnDraggableObject(DraggableObject draggableObject)
        {
            if (draggableObject != null)
            {
                draggableObject.SetupCamera(Camera.main);
                draggableObject.OnEndDragging += SaveDraggablePointData;
            }
        }


        private void UnsubscribeOnDraggableObject(DraggableObject draggableObject)
        {
            if (draggableObject != null)
            {
                draggableObject.OnEndDragging -= SaveDraggablePointData;

                Destroy(draggableObject.gameObject);
            }
        }

        #endregion



        #region Events handlers

        private void LengthChangeInput_OnValueChange(float value)
        {
            editorObject.Length = value;
            editorObject.RefreshData();
        }


        private void SegmentsShiftChangeInput_OnValueChange(float value)
        {
            editorObject.SegmentsShift = value;
            editorObject.RefreshData();
        }


        private void EndPositionToCenterButton_OnClick()
        {
            if (endConnectedDraggableObject != null)
            {
                endConnectedDraggableObject.transform.position = editorObject.ConnectedObjectPosition;
            }

            if (hookConnectedDraggableObject != null)
            {
                hookConnectedDraggableObject.transform.position = editorObject.HookConnectedObjectPosition;
            }

            SaveDraggablePointData();
        }


        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            actualLengthText.text = editorObject.ActualRopeLength.ToString();
        }


        private void SaveDraggablePointData()
        {
            editorObject.EndAnchorOffset = editorObject.ConnectedObjectPosition - endConnectedDraggableObject.transform.position;
            editorObject.HookAnchorOffset = editorObject.HookConnectedObjectPosition - hookConnectedDraggableObject.transform.position;
            editorObject.RefreshData();
        }


        private void SphericalTrajectory_OnValueChange(bool value)
        {
            editorObject.IsSphericalTrajectory = value;
            editorObject.RefreshData();
        }

        #endregion
    }
}
