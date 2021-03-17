using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Levels;


namespace Drawmasters.LevelConstructor
{
    public class JointInterface : InspectorExtensionBase, IDeinitializable
    {
        #region Fields

        private static readonly Vector3 initialOffset = new Vector3(25f, 25f, 0f);

        public Action<float> OnHeightChanged;


        [SerializeField] private LayoutElement mainLayout = default;
        [SerializeField] private BoolInputUi jointBoolInput = default;

        [SerializeField] private Button addJointButton = default;
        [SerializeField] private LayoutElement addJointLayoutElement = default;

        [SerializeField] private Float3InputRemovableUi float3InputPrefab = default;
        [SerializeField] private JointAnchor jointAnchorPrefab = default;

        private List<Float3InputRemovableUi> jointsAnchors = default;

        private Dictionary<Float3InputRemovableUi, JointAnchor> inputData = default;

        private bool isEditEnabled = false;

        private EditorLevelObject levelObject;
        private float? initialHeight;

        #endregion



        #region Properties

        private float InitialHeight
        {
            get
            {
                if (initialHeight == null)
                {
                    initialHeight = mainLayout.preferredHeight;
                }

                return initialHeight.Value;
            }
        }


        public float AdditionalHeight
        {
            get
            {
                float height = 0f;

                if (isEditEnabled)
                {
                    height += addJointLayoutElement.preferredHeight;

                    jointsAnchors.ForEach(anchor => height += anchor.Height);
                }

                return height;
            }
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            jointsAnchors.Clear();

            foreach (var pair in inputData)
            {
                if (pair.Value != null &&
                    pair.Value.gameObject != null)
                {
                    Destroy(pair.Value.gameObject);
                }
            }

            inputData.Clear();
        }

        #endregion



        #region Overrided methods

        public override void Init(EditorLevelObject _levelObject)
        {
            levelObject = _levelObject;
            isEditEnabled = false;
            jointsAnchors = new List<Float3InputRemovableUi>();
            inputData = new Dictionary<Float3InputRemovableUi, JointAnchor>();

            jointBoolInput.Init("Joint edit", isEditEnabled);


            List<Vector3> initialJoints = levelObject.GetData().jointsPoints;
            if (initialJoints != null)
            {
                foreach (var v in initialJoints)
                {
                    AddElement(v, false);
                }
            }

            RepaintUi();
        }


        protected override void SubscribeOnEvents()
        {
            jointBoolInput.OnValueChange += JointBoolInput_OnValueChange;

            addJointButton.onClick.AddListener(AddJointButton_OnClick);

            SelectedObjectChange.Subscribe(OnSelectedObjectsChanged);
        }




        protected override void UnsubscribeFromEvents()
        {
            jointBoolInput.OnValueChange -= JointBoolInput_OnValueChange;

            addJointButton.onClick.RemoveListener(AddJointButton_OnClick);

            SelectedObjectChange.Unsubscribe(OnSelectedObjectsChanged);
        }


        protected override void OnDisable()
        {
            Deinitialize();

            base.OnDisable();
        }

        #endregion



        #region Methods

        private void RepaintUi()
        {
            mainLayout.preferredHeight = InitialHeight + AdditionalHeight;

            CommonUtility.SetObjectActive(addJointLayoutElement.gameObject, isEditEnabled);
            jointsAnchors.ForEach(joinInput => CommonUtility.SetObjectActive(joinInput.gameObject, isEditEnabled));


            OnHeightChanged?.Invoke(AdditionalHeight);
        }


        private void RepaintDrawers()
        {
            foreach (var pair in inputData)
            {
                CommonUtility.SetObjectActive(pair.Value.gameObject, isEditEnabled);
            }
        }


        private void RemoveElement(Float3InputRemovableUi element)
        {
            int pointIndex = jointsAnchors.IndexOf(element);
            if (pointIndex >= 0 &&
                pointIndex < jointsAnchors.Count)
            {
                levelObject.JointsPoints.RemoveAt(pointIndex);
            }

            element.OnRemoved -= Anchor_OnRemoved;
            element.OnValueChanged -= Anchor_OnValueChanged;

            if (inputData.TryGetValue(element, out JointAnchor jointAnchor))
            {
                jointAnchor.OnPositionChanged -= JointAnchor_OnLocalPositionChanged;

                Destroy(jointAnchor.gameObject);
            }

            jointsAnchors.Remove(element);
            inputData.Remove(element);

            Destroy(element.gameObject);

            RepaintUi();
            RepaintDrawers();
        }


        private void AddElement(Vector3 localElementPosition, bool isNewObject)
        {
            if (isNewObject)
            {
                levelObject.JointsPoints.Add(localElementPosition);
            }

            Vector3 globalElementPosition = levelObject.transform.position + localElementPosition;

            Float3InputRemovableUi control = Instantiate(float3InputPrefab);
            control.name = float3InputPrefab.name;
            control.transform.localPosition = Vector3.zero;
            control.transform.SetParent(gameObject.transform, false);
            control.transform.localScale = Vector3.one;

            JointAnchor anchorDrawer = Instantiate(jointAnchorPrefab);
            anchorDrawer.name = jointAnchorPrefab.name;
            anchorDrawer.transform.SetParent(null);
            anchorDrawer.transform.position = globalElementPosition;

            anchorDrawer.OnPositionChanged += JointAnchor_OnLocalPositionChanged;
            control.OnValueChanged += Anchor_OnValueChanged;
            control.OnRemoved += Anchor_OnRemoved;

            anchorDrawer.Init(control, levelObject);
            control.Init($"Joint {jointsAnchors.Count}", 0);
            control.SetCurrentValue(localElementPosition);

            inputData.Add(control, anchorDrawer);
            jointsAnchors.Add(control);

            RepaintUi();
            RepaintDrawers();
        }


        private void AddElement() => AddElement(initialOffset, true);


        private void ChangeControlPosiion(Float3InputRemovableUi control,
                                          Vector3 position)
        {
            LevelObjectData data = levelObject.GetData();

            int poinIndex = jointsAnchors.IndexOf(control);
            if (poinIndex >= 0 &&
                poinIndex < data.jointsPoints.Count)
            {
                data.jointsPoints[poinIndex] = position;
            }
            else
            {
                CustomDebug.Log("Wrong position change");
            }
        }

        #endregion



        #region Events handlers

        private void AddJointButton_OnClick()
        {
            AddElement();
        }


        private void JointBoolInput_OnValueChange(bool value)
        {
            isEditEnabled = value;

            RepaintUi();
            RepaintDrawers();
        }


        private void Anchor_OnValueChanged(Float3InputRemovableUi element,
                                           Vector3 localPosition)
        {
            if (inputData.TryGetValue(element, out JointAnchor jointAnchor))
            {
                jointAnchor.ChangeLocalPosition(levelObject.transform.position + localPosition);

                ChangeControlPosiion(element, localPosition);
            }
        }


        private void Anchor_OnRemoved(Float3InputRemovableUi element)
        {
            RemoveElement(element);
        }


        private void JointAnchor_OnLocalPositionChanged(Float3InputRemovableUi control,
                                                        Vector3 position)
        {
            if (inputData.TryGetValue(control, out JointAnchor jointAnchor))
            {
                Vector3 localPosition = position - levelObject.transform.position;

                control.SetCurrentValue(localPosition.x,
                                        localPosition.y,
                                        localPosition.z);

                ChangeControlPosiion(control, localPosition);
            }
        }


        private void OnSelectedObjectsChanged(List<EditorLevelObject> objects)
        {
            bool isObjectDeselect = !objects.Contains(levelObject);

            if (isObjectDeselect)
            {
                Deinitialize();
            }
        }

        #endregion
    }
}
