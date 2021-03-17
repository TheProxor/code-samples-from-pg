using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "EnemyAnimationNames",
                     menuName = NamingUtility.MenuItems.IngameSettings + "EnemyAnimationNames")]
    public class EnemyAnimationNames : ScriptableObject
    {
        #region Helpers

        [Serializable]
        public class ComplexAnimation
        {
            [SpineAnimation(dataField = "skeletonAnimation")]
            public string start = default;

            [SpineAnimation(dataField = "skeletonAnimation")]
            public string main = default;

            [SpineAnimation(dataField = "skeletonAnimation")]
            public string end = default;
        }

        #endregion



        #region Fields

        #pragma warning disable 0414
        
        // for reflection only
        [SerializeField] private SkeletonDataAsset skeletonAnimation = default;

        #pragma warning restore 0414 

        [Header("Idle")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private string idleAnimationName = default;

        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private List<string> idleSpecialAnimationNames = default;

        [Header("Panic")]
        [SerializeField] private List<ComplexAnimation> panicAnimations = default;

        [Header("Face pain")]
        [SerializeField] private List<ComplexAnimation> facePainAnimations = default;

        [Header("Dance")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private List<string> danceAnimations = default;

        [Header("Laught")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private string[] laughtAnimations = default;
        

        [Header("Angry")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private List<string> angryAnimations = default;

        [Header("Face death")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private List<string> faceDeathAnimations = default;

        [SerializeField] private List<ComplexAnimation> faceBurnAnimations = default;
        
        [Header("Cry")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private string defeatAnimation = default;

        [Header("Win")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private List<string> winAnimations = default;

        [Header("Burn")]
        [SpineAnimation(dataField = "skeletonAnimation")]
        [SerializeField] private string burnAnimationName = default;

        [SpineSlot(dataField = "skeletonAnimation")]
        public string[] slotsToRefreshOnDeath = default;

        #endregion



        #region Properties

        public string IdleAnimationName => idleAnimationName;

        public string BurnAnimationName => burnAnimationName;

        public string RandomSpecialIdleAnimation => idleSpecialAnimationNames.RandomObject();

        public ComplexAnimation RandomPanicAnimation => panicAnimations.RandomObject();

        public ComplexAnimation RandomFacePainAnimation => facePainAnimations.RandomObject();

        public string RandomDanceAnimation => danceAnimations.RandomObject();

        public string RandomLaughtAnimation => laughtAnimations.RandomObject();

        public string RandomAngryAnimation => angryAnimations.RandomObject();

        public string RandomFaceDeathAnimation => faceDeathAnimations.RandomObject();

        public ComplexAnimation RandomFaceBurnAnimation => faceBurnAnimations.RandomObject();

        public string DefeatAnimation => defeatAnimation;

        public string RandomWinAnimation => winAnimations.RandomObject();

        #endregion
    }
}
