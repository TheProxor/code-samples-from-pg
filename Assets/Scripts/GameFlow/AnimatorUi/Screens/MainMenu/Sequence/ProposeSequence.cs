using System.Collections.Generic;
using UnityEngine.Events;

namespace Drawmasters.Ui
{
    public class ProposeSequence : IInitializable, IDeinitializable
    {
        #region Fields

        private Queue<IProposeSequenceElement> proposeSequenceElementsQueue;

        public readonly UnityEvent OnComplete = new UnityEvent();

        private List<IProposeSequenceElement> sequenceElements;

        #endregion



        #region Properties

        public bool IsPlaying { get; private set; }

        public bool IsPaused  { get; private set; }

        #endregion



        #region Class Lifecycle

        public void Initialize()
        {
            proposeSequenceElementsQueue = new Queue<IProposeSequenceElement>();
            sequenceElements = new List<IProposeSequenceElement>();
        }


        public void Deinitialize()
        {
            foreach (var element in sequenceElements)
            {
                element.StopSequenceElementExecution();
            }

            OnComplete.RemoveAllListeners();

            sequenceElements.Clear();
            proposeSequenceElementsQueue.Clear();

            IsPaused = false;
            IsPlaying = false;
        }

        #endregion



        #region Methods

        public void Append(IProposeSequenceElement proposeSequenceElement)
        {
            if (IsPlaying)
            {
                CustomDebug.Log("You can't add sequence element while it's playing");
            }

            proposeSequenceElement.OnCompleteSequenceElement.RemoveAllListeners();
            proposeSequenceElement.OnCompleteSequenceElement.AddListener(MoveNext);

            proposeSequenceElementsQueue.Enqueue(proposeSequenceElement);
            sequenceElements.Add(proposeSequenceElement);
        }


        public void Clear() 
        {
            if (IsPlaying)
            {
                CustomDebug.Log("You can't clear sequence while it's playing");
            }

            proposeSequenceElementsQueue.Clear();
        }


        public void Play()
        {
            IsPlaying = true;

            MoveNext();
        }


        public void Pause() =>
            IsPaused = true;


        public void Unpause() =>
            IsPaused = false;


        private void MoveNext()
        {
            if (proposeSequenceElementsQueue.Count == 0 || proposeSequenceElementsQueue.Peek() == null)
            {
                OnComplete?.Invoke();
                OnComplete.RemoveAllListeners();

                IsPlaying = false;
            }
            else
            {
                proposeSequenceElementsQueue.Dequeue().StartSequenceElementExecution(this);
            }
        }

        #endregion
    }
}