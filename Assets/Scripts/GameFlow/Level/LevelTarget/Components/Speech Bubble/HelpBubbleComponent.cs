using Drawmasters.Pool;
using UnityEngine;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public class HelpBubbleComponent : SpeechBubbleComponent
    {
        #region Overrided methods        

        public override void Enable()
        {
            RagdollLevelTargetComponent.OnRagdollApplied += RagdollLevelTargetComponent_OnRagdollApplied;
            bubble = CreateBubble();
            bubble.SetRuntimeHandler<SpeechBubbleHelpHandler>();
            ShowBubble();
        }


        public override void Disable()
        {
            RagdollLevelTargetComponent.OnRagdollApplied -= RagdollLevelTargetComponent_OnRagdollApplied;
            HideAndDestroyBubble(true);
        }

        #endregion


        #region Events handlers

        private void RagdollLevelTargetComponent_OnRagdollApplied(LevelTarget otherTarget)
        {
            if (levelTarget.Equals(otherTarget))
            {
                HideAndDestroyBubble();
            }
        }

        #endregion
    }
}

