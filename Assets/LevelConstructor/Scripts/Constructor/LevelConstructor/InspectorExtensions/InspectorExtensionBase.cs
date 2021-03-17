using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public abstract class InspectorExtensionBase : MonoBehaviour
    {
        public abstract void Init(EditorLevelObject levelObject);


        protected abstract void SubscribeOnEvents();


        protected abstract void UnsubscribeFromEvents();


        private void OnEnable()
        {
            SubscribeOnEvents();
        }


        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
        }
    }
}
