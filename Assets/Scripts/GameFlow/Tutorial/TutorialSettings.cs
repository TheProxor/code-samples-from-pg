using System;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialSettings",
                    menuName = NamingUtility.MenuItems.IngameSettings + "TutorialSettings")]
    public class TutorialSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        public class GameTutorialData
        {
            public TutorialType type = default;
            public GameMode mode = default;

            public SkeletonDataAsset animationDataAsset = default;
            [SpineAnimation(dataField: "animationDataAsset")] public string startAnimation = default;

            public float buttonEnableDelay = default;
        }


        [Serializable]
        public class FingersTutorialData
        {
            public TutorialType type = default;

            public Animator fingerAnimator = default;
        }


        [Serializable]
        public class TutorialPrefabsData
        {
            public TutorialType type = default;
            public Animator animator = default;
        }

        #endregion



        #region Fields

        [SerializeField] private GameTutorialData[] data = default;
        [SerializeField] private FingersTutorialData[] fingerData = default;
        [SerializeField] private TutorialPrefabsData[] prefabsData = default;

        #endregion



        #region Methods

        public FingersTutorialData FindFingerTutorialData(TutorialType type)
        {
            FingersTutorialData foundData = Array.Find(fingerData, e => e.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"No data for type {type} found in {this}");
            }

            return foundData;
        }


        public GameTutorialData FindGameData(TutorialType type)
        {
            GameTutorialData foundData = Array.Find(data, e => e.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"No data for type {type} found in {this}");
            }

            return foundData;
        }


        public Animator FindTutorialPrefabsAnimator(TutorialType type)
        {
            TutorialPrefabsData foundData = Array.Find(prefabsData, e => e.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"No data for type {type} found in {this}");
            }

            return foundData == null ? default : foundData.animator;
        }


        public TutorialType FindModeTutorialType(GameMode mode)
        {
            GameTutorialData foundData = Array.Find(data, e => e.mode == mode);
            return foundData == null ? TutorialType.None : foundData.type;
        }

        #endregion
    }
}
