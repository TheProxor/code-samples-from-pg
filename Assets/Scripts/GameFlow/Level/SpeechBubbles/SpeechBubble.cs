using UnityEngine;

namespace Drawmasters.Levels
{
    public class SpeechBubble : MonoBehaviour, IInitializable, IDeinitializable
    {

        #region Fields

        [SerializeField] private SpeechBubbleRuntimeHandler[] handlers = default;

        #endregion



        #region Properties

        public SpeechBubbleRuntimeHandler CurrentHandler { get; private set; }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            foreach (var component in handlers)
            {
                component.Initialize();
            }
        }

        #endregion



        #region IDeinitializable

        public virtual void Deinitialize()
        {
            foreach (var component in handlers)
            {
                component.Deinitialize();
            }
        }

        #endregion



        #region Methods

        public void SetRuntimeHandler<T>() where T : SpeechBubbleRuntimeHandler
        {
            SpeechBubbleRuntimeHandler result;
            result = handlers.Find(x => x is T);
            if(result == null)
            {
                CustomDebug.LogError($"Attempt to use SpeechBubbleContainer not attached to prefab '{gameObject.name}'");
                return;
            }

            CurrentHandler = result;

            ActivateSpeechBubble();
        }

        private void ActivateSpeechBubble()
        {
            foreach (var speechBubble in handlers)
            {
                speechBubble.gameObject.SetActive(speechBubble == CurrentHandler);
            }
        }

        #endregion
    }
}
