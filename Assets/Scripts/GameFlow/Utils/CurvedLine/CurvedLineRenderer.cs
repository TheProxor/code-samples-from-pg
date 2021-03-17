//MIT License

//Copyright(c) 2016 Süleyman Yasir KULA

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

// Minor API updates by Vladislav.k
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drawmasters.Utils
{
	public class CurvedLineRenderer
	{
		private readonly float lineSegmentSize;

		private Vector3[] linePoints = Array.Empty<Vector3>();

		private Vector3[] linePositions = Array.Empty<Vector3>();
		private Vector3[] linePositionsOld = Array.Empty<Vector3>();
		private Vector3[] smoothedPoints = Array.Empty<Vector3>();

		private readonly bool shouldSmooth;


		public List<Vector3> VertexPositions
		{
			get
			{
				if (LineRenderer == null)
				{
					return new List<Vector3>();
				}

				List<Vector3> lineTrajectory = new List<Vector3>(LineRenderer.positionCount);

				for (int i = 0; i < LineRenderer.positionCount; i++)
				{
					lineTrajectory.Add(LineRenderer.GetPosition(i));
				}

				return lineTrajectory;
			}
		}

		public LineRenderer LineRenderer { get; }


		public CurvedLineRenderer(LineRenderer _lineRenderer, float _lineSegmentSize)
        {
			LineRenderer = _lineRenderer;
			lineSegmentSize = _lineSegmentSize;

		}


		public void StartDrawing()
		{
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
		}


        public void FinishDrawing()
		{
			MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
		}


		public void SetPoints(Vector3[] points)
		{
			linePoints = points;

			linePositions = new Vector3[linePoints.Length];

			for (int i = 0; i < linePoints.Length; i++)
			{
				linePositions[i] = linePoints[i];
			}
		}



		private void Draw()
		{
			//create old positions if they dont match
			if (linePositionsOld.Length != linePositions.Length)
			{
				linePositionsOld = new Vector3[linePositions.Length];
			}

			//check if line points have moved
			bool moved = false;
			for (int i = 0; i < linePositions.Length; i++)
			{
				//compare
				if (linePositions[i] != linePositionsOld[i])
				{
					moved = true;
				}
			}

			//update if moved
			if (moved)
			{
				//get smoothed values
				smoothedPoints = LineSmoother.SmoothLine(linePositions, lineSegmentSize);

                //set line settings
                LineRenderer.positionCount = (smoothedPoints.Length);
                LineRenderer.SetPositions(smoothedPoints);
            }
		}


		private void MonoBehaviourLifecycle_OnUpdate(float deltaTime) =>
			Draw();
	}
}