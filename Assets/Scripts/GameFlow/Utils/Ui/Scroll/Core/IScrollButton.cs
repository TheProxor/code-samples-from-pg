using UnityEngine;
using UnityEngine.EventSystems;


namespace Drawmasters.Utils.Ui
{
    public interface IScrollButton
    {
        RectTransform ButtonRectTransform { get; }

        Rect GetButtonWorldRect(Vector3 scale);

        void OnPointerUp(PointerEventData pointerEventData);

        /// <summary>
        /// Need a more explicit method name. Here should be onClick invoke and button release
        /// </summary>
        /// <param name="pointerEventData"></param>
        void OnShouldClick(PointerEventData pointerEventData);
    }
}