using System.Collections.Generic;
using Drawmasters.Levels.Objects;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class MovementPathDrawer : DrawerBase<EditorDynamicPlank>
    {
        #region Fields

        [SerializeField] private LineRenderer pathDrawer = default;
        [SerializeField] private Color lineColor = default;

        private List<Vector3> positions = new List<Vector3>();

        #endregion



        #region Methods

        protected override void Initialize()
        {
            base.Initialize();

            pathDrawer.material.color = lineColor;
        }


        protected override void Draw()
        {
            positions.Clear();
            positions.Add(selectedObject.transform.position);

            foreach (var point in selectedObject.Path)
            {
                positions.Add(point.position);
            }

            if (selectedObject.CycleType == DynamicPlank.CycleType.Circle)
            {
                positions.Add(selectedObject.transform.position);
            }

            pathDrawer.positionCount = positions.Count;
            pathDrawer.SetPositions(positions.ToArray());
        }

        #endregion
    }
}
