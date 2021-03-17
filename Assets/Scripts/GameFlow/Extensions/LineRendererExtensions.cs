using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Utils
{
    public static class LineRendererExtensions 
    {
        public static List<Vector3> GetCurrentPoints(this LineRenderer lineRenderer)
        {
			if (lineRenderer == null)
			{
				return new List<Vector3>();
			}

			List<Vector3> lineTrajectory = new List<Vector3>(lineRenderer.positionCount);

			for (int i = 0; i < lineRenderer.positionCount; i++)
			{
				lineTrajectory.Add(lineRenderer.GetPosition(i));
			}

			return lineTrajectory;
		}
    }
}