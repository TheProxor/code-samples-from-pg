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
using UnityEngine;
using System.Collections.Generic;


namespace Drawmasters.Utils
{
    public static class LineSmoother
    {
        public static Vector3[] SmoothLine(Vector3[] inputPoints, float segmentSize)
        {
            //create curves
            AnimationCurve curveX = new AnimationCurve();
            AnimationCurve curveY = new AnimationCurve();
            AnimationCurve curveZ = new AnimationCurve();

            //create keyframe sets
            Keyframe[] keysX = new Keyframe[inputPoints.Length];
            Keyframe[] keysY = new Keyframe[inputPoints.Length];
            Keyframe[] keysZ = new Keyframe[inputPoints.Length];

            //set keyframes
            for (int i = 0; i < inputPoints.Length; i++)
            {
                keysX[i] = new Keyframe(i, inputPoints[i].x);
                keysY[i] = new Keyframe(i, inputPoints[i].y);
                keysZ[i] = new Keyframe(i, inputPoints[i].z);
            }

            //apply keyframes to curves
            curveX.keys = keysX;
            curveY.keys = keysY;
            curveZ.keys = keysZ;

            //smooth curve tangents
            for (int i = 0; i < inputPoints.Length; i++)
            {
                curveX.SmoothTangents(i, 0);
                curveY.SmoothTangents(i, 0);
                curveZ.SmoothTangents(i, 0);
            }

            //list to write smoothed values to
            List<Vector3> lineSegments = new List<Vector3>();

            //find segments in each section
            for (int i = 0; i < inputPoints.Length; i++)
            {
                //add first point
                lineSegments.Add(inputPoints[i]);

                //make sure within range of array
                if (i + 1 < inputPoints.Length)
                {
                    //find distance to next point
                    float distanceToNext = Vector3.Distance(inputPoints[i], inputPoints[i + 1]);

                    //number of segments
                    int segments = (int)(distanceToNext / segmentSize);

                    //add segments
                    for (int s = 1; s < segments; s++)
                    {
                        //interpolated time on curve
                        float time = (s / (float)segments) + i;

                        //sample curves to find smoothed position
                        Vector3 newSegment = new Vector3(curveX.Evaluate(time), curveY.Evaluate(time), curveZ.Evaluate(time));

                        //add to list
                        lineSegments.Add(newSegment);
                    }
                }
            }

            return lineSegments.ToArray();
        }

    }
}
