using UnityEngine;
using Drawmasters.ServiceUtil;
using Modules.General;

namespace Drawmasters.Levels
{
    public class LoserBubbleComponent : SpeechBubbleComponent
    {
        #region Fields

        private const string HeadLimbName = "head";
        private const float Delay = 2f;

        #endregion



        #region Overrided methods        

        public override void Enable()
        {
            if (GameServices.Instance.LevelEnvironment.Context.Mode.IsHitmastersLiveOps())
            {
                return;
            }

            ProjectileEnemiesSmashComponent.OnSmash += OnSmash;

            levelTarget.OnShouldApplyRagdoll += OnApplyRagdoll;
        }


        public override void Disable()
        {
            ProjectileEnemiesSmashComponent.OnSmash -= OnSmash;

            levelTarget.OnShouldApplyRagdoll -= OnApplyRagdoll;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            HideAndDestroyBubble(true);
        }

        private void MoveBubbleToHead(LevelTarget target)
        {
            var bone = target.Limbs.Find(x => x.RootBoneName == HeadLimbName);
            if (bone != null)
            {
                MoveBubble(bone.transform.position);
            }
            else
            {
                CustomDebug.LogError($"Enemy '{levelTarget.gameObject.name}' has no haed limb woth root name '{HeadLimbName}'");
            }
        }
        #endregion



        #region Methods

        private void MoveBubble(Vector3 position) => bubble.transform.position = position;

        #endregion



        #region Events handlers

        private void OnSmash(LevelTarget anotherLevelTarget)
        {
            if (anotherLevelTarget != levelTarget)
            {
                return;
            }

            ProjectileEnemiesSmashComponent.OnSmash -= OnSmash;
            bubble = CreateBubble();
            bubble.SetRuntimeHandler<SpeechBubbleLoserHandler>();

            MoveBubbleToHead(levelTarget);

            SpeechBubbleLoserHandler hanlder = bubble.CurrentHandler as SpeechBubbleLoserHandler;
            hanlder.SetBubbleOrientation();

            ShowBubble();

            Scheduler.Instance.CallMethodWithDelay(this, () => HideAndDestroyBubble(false), Delay);
        }

        private void OnApplyRagdoll(LevelTarget levelTarget) => HideAndDestroyBubble(true);

        #endregion
    }
}

