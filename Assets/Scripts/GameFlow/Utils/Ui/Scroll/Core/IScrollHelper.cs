using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace Drawmasters.Utils.Ui
{
    public interface IScrollHelper
    {
        Canvas MainCanvas { get; }

        RectTransform ViewportRectTransform { get; }

        void AddCallback(UnityAction<BaseEventData> callback, EventTriggerType eventTriggerType);

        void RemoveCallback(UnityAction<BaseEventData> callback, EventTriggerType eventTriggerType);

        void AddOnValueChangedCallback(UnityAction<Vector2> callback);

        void RemoveOnValueChangedCallback(UnityAction<Vector2> callback);

        Vector3 GetElementViewPosition(RectTransform target);
    }
}
