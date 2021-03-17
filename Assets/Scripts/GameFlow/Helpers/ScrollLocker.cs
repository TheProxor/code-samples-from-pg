using UnityEngine;
using UnityEngine.EventSystems;

namespace Drawmasters.Helpers
{
    public class ScrollLocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private MonoBehaviour dragHandler;

        private void Start()
        {
            dragHandler = transform.GetComponentInParent<IDragHandler>() as MonoBehaviour;

            if(dragHandler == null)
            {
                CustomDebug.Log("Scroll Locker could not found IDragHandler implementation in parent transforms");
                Destroy(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            dragHandler.enabled = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            dragHandler.enabled = true;
        }
    }
}