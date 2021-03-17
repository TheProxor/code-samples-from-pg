using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Drawmasters
{
    public static class EventSystemController
    {
        #region Fields

        private static EventSystem eventSystem;

        private static readonly List<object> pauseSenders = new List<object>();

        #endregion



        #region Properties

        public static PointerEventData CurrentPointerEventData =>
            eventSystem == null ? default : new PointerEventData(eventSystem);


        public static GameObject CurrentSelectedGameObject =>
            eventSystem == null ? default : eventSystem.currentSelectedGameObject;

        #endregion



        #region Methods

        public static void SetupEventSystem(EventSystem eventSystemValue)
        {
            eventSystem = eventSystemValue;

            if (eventSystem == null)
            {
                CustomDebug.Log("No event system found");
            }
        }


        public static void EnableEventSystem()
        {
            if (eventSystem != null)
            {
                eventSystem.enabled = true;
            }
        }


        public static bool IsPointerOverGameObject(int pointerId)
        {
            bool result = default;

            if (eventSystem != null)
            {
                result = eventSystem.IsPointerOverGameObject(pointerId) ||
                         eventSystem.currentSelectedGameObject != null;
            }

            return result;
        }


        public static bool SetSelectedGameObject(GameObject obj, PointerEventData pointerEventData)
        {
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(obj, pointerEventData);
            }

            return eventSystem != null;
        }


        public static bool IsPointerOverGameObject() => IsPointerOverGameObject(-1);


        public static void SetSystemEnabled(bool enable, object target)
        {
            if (isDebug)
            {
                CustomDebug.Log($"Target: {target} set input: {enable}");
            }
            
            if (enable)
            {
                pauseSenders.Remove(target);
            }
            else
            {
                pauseSenders.AddExclusive(target);
            }

            pauseSenders.RemoveAll(e => e.IsNull()); // hotfix wrong lc;

            if (eventSystem != null)
            {
                eventSystem.enabled = pauseSenders.Count == 0;
            }
        }


        public static void SimulateTouch(GameObject go) =>
            ExecuteEvents.Execute(go, CurrentPointerEventData, ExecuteEvents.submitHandler);
        
        #endregion



        #region Editor

        private static bool isDebug = false;

        #endregion
    }
}
