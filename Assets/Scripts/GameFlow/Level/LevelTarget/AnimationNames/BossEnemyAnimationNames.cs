using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class BossEnemyAnimationNames : EnemyAnimationNames
    {
        #region Fields

        [Header("Mockery")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private List<string> mockeryAnimations = default;

        [Header("Mockery")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private string[] greetingMockeryAnimations = default;

        [Header("Shot")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private string[] shotAnimationsAnimations = default;

        [Header("Projectiles Destroy")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private string[] projectileDestroyAnimations = default;


        [Header("Stage")]
        [SpineAnimation(dataField = "skeletonAnimation")] public string commingAnimation = default;
        [SpineAnimation(dataField = "skeletonAnimation")] public string endCommingAnimation = default;

        [SpineAnimation(dataField = "skeletonAnimation")] public string beginLeavingAnimation = default;
        [SpineAnimation(dataField = "skeletonAnimation")] public string leavingAnimation = default;

        #endregion



        #region Properties

        public string RandomMockeryAnimation =>
            mockeryAnimations.RandomObject();

        public string RandomGreetingMockeryAnimations =>
            greetingMockeryAnimations.RandomObject();
        
        public string RandomShotAnimation =>
            shotAnimationsAnimations.RandomObject();

        public string RandomProjectileDestroyAnimation =>
            projectileDestroyAnimations.RandomObject();

        #endregion
    }
}
