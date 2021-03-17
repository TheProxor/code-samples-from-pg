using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class BonusLevelAnimationComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        [SerializeField] private SkeletonAnimation characterAnimation = default;

        private static readonly List<string> Animations = new List<string>{ "bonus_gems_airborne_panic", "bonus_gems_airborne_fun" };

        private BonusLevelController controller;

        #endregion



        public override void Enable()
        {
            controller = GameServices.Instance.LevelControllerService.BonusLevelController;
            
            characterAnimation.AnimationState.SetAnimation(0, Animations.RandomObject(), true);
            
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }



        public override void Disable()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }
        
        
        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (characterAnimation != null &&
                controller != null)
            {
                characterAnimation.timeScale = controller.CustomTimeScale;
            }
        }
    }
}

