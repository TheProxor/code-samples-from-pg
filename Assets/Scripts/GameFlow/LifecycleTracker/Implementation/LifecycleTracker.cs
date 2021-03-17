using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Drawmasters
{
    public class LifecycleTracker : ILifecycleTracker
    {
        #region Fields

        private bool wasInitialized;
        private bool wasDeinitialized;
        protected readonly string message;

        protected string previousDeinitializeCallStack;

        #endregion



        #region Ctor

        public LifecycleTracker(string message = null)
        {
            this.message = message;
        }

        #endregion



        #region ILifecycleTracker

        public void Initialize()
        {
            if (wasInitialized)
            {
                #if UNITY_EDITOR
                    throw new MultipleInitializeException(message);
                #endif
            }

            wasInitialized = true;
            wasDeinitialized = false;
        }
        

        public void Deinitialize()
        {
            if (wasDeinitialized)
            {
                #if UNITY_EDITOR
                    throw new MultipleDeinitializeException(previousDeinitializeCallStack);
                #endif
            }

            if (!wasInitialized)
            {
                #if UNITY_EDITOR
                    throw new InitializeRequiredException(message);
                #endif
            }

            wasDeinitialized = true;
            wasInitialized = false;
            previousDeinitializeCallStack = GetStackFrame();
        }
        

        public void OnDestroy()
        {
            //TODO think ???
            // if (!wasInitialized)
            // {
            //     throw new InitializeRequiredException(message);
            // }

            if (wasInitialized && !wasDeinitialized)
            {
                #if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPlaying)
                    {
                        CustomDebug.Log($"There must be a DeinitializeRequiredException {message}");
                    }
                #endif
            }
        }

        #endregion



        #region Protected methods

        protected string GetStackFrame()
        {
            string stack = "";
            
            StackTrace trace = new StackTrace();
            
            StackFrame[] frames = trace.GetFrames();
            if (frames != null)
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    stack += GetStackFrameDebugInfo(frames[i]);
                }

                stack += "\n" + "\n" + "\n";
            }
            else
            {
                stack = "Not found stack";
            }

            return stack;
        }


        protected string GetStackFrameDebugInfo(StackFrame frame)
        {
            MethodBase method = frame.GetMethod();

            string typeName = method.DeclaringType.FullName;
            string methodName = method.Name;
            string fileName = frame.GetFileName();
            string lineNumber = frame.GetFileLineNumber().ToString();

            return $"{typeName}.{methodName} at ({fileName}, line: {lineNumber})" + '\n';
        }

        #endregion
    }
}