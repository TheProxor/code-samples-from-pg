using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Drawmasters.Utils
{
    public static class PressButtonUtility
    {
        [Serializable]
        public class Data
        {
            public float beginInteractibleDelay = default;
            public float tapDelay = default;
            public float tapDuration = default;
            public float afterTapDelay = default;
        }

        public static void ImmitateButtonUp(Button target, PointerEventData pointerEventData)
        {
            if (pointerEventData.selectedObject == target.gameObject)
            {
                EventSystemController.SetSelectedGameObject(null, pointerEventData);

                target.OnPointerExit(pointerEventData);
                target.OnPointerUp(pointerEventData);
                target.OnDeselect(pointerEventData);
            }
        }


        public static void ImmitateButtonClick(Button target, PointerEventData pointerEventData)
        {
            if (pointerEventData.selectedObject == target.gameObject)
            {
                target.onClick?.Invoke();
                ImmitateButtonUp(target, pointerEventData); 

                EventSystemController.SetSelectedGameObject(null, pointerEventData);
            }
        }
        

        public static IEnumerator PressRoutine(Button target, Data data, Action onPress = default)
        {
            EventSystemController.SetSystemEnabled(false, target);

            yield return new WaitForSeconds(data.tapDelay);

            onPress?.Invoke();

            target.animator.SetTrigger(target.animationTriggers.pressedTrigger);

            yield return new WaitForSeconds(data.tapDuration);
            target.animator.SetTrigger(target.animationTriggers.highlightedTrigger);
            target.animator.SetTrigger(target.animationTriggers.normalTrigger);

            yield return new WaitForSeconds(data.afterTapDelay);
            target.onClick?.Invoke();

            EventSystemController.SetSystemEnabled(true, target);
        }
    }
}
