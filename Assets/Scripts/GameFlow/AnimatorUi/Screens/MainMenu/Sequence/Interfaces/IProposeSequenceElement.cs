using UnityEngine.Events;


namespace Drawmasters.Ui
{
    public interface IProposeSequenceElement
    {
        UnityEvent OnCompleteSequenceElement { get; }


        void StartSequenceElementExecution(ProposeSequence sequence);

        void StopSequenceElementExecution();
    }
}
