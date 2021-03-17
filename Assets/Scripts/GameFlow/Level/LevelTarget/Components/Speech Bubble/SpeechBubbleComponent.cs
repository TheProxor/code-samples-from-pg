using Drawmasters.Pool;
using UnityEngine;

namespace Drawmasters.Levels
{
    public abstract class SpeechBubbleComponent : LevelTargetComponent
    {
        #region Fields

        protected SpeechBubble bubble;

        #endregion



        #region Protected methods

        protected SpeechBubble CreateBubble()
        {
            Vector3 position = levelTarget.transform.position;

            bubble = Content.Management.CreateSpeechBubble(levelTarget.transform, position);
            bubble.Initialize();

            return bubble;
        }

        protected void DestroyBubble()
        {
            ComponentPool pool = PoolManager.Instance.GetComponentPool(bubble);
            pool.Push(bubble);

            bubble.Deinitialize();
            bubble = null;
        }


        protected void HideAndDestroyBubble(bool isImmediately = false)
        {
            if (bubble != null && bubble.CurrentHandler != null)
            {
                bubble.CurrentHandler.Hide(DestroyBubble, isImmediately);
            }
        }

        protected void ShowBubble()
        {
            if (bubble != null && bubble.CurrentHandler != null)
            {
                bubble.CurrentHandler.Show();
            }
        }

        protected void HideBubble(bool isImmediately = false)
        {
            if (bubble != null && bubble.CurrentHandler != null)
            {
                bubble.CurrentHandler.Hide(null, isImmediately);
            }
        }

        #endregion

    }
}

