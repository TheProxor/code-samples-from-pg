using System;
using Spine.Unity;
using Drawmasters.Effects;
using Drawmasters.Helpers;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class ResultPetCompleteBehaviour
    {
        #region Nested types

        [Serializable]
        public class Data
        {
            public GameObject mainRoot = default;

            public SpineAnimationSequencePlayer.Data sequencePlayerData = default;

            public Transform fxRoot = default;
        }

        #endregion



        #region Fields

        private readonly Data data;
        private readonly ILevelEnvironment levelEnvironment;

        private readonly SpineAnimationSequencePlayer animationPlayer;


        private bool wasPetUsedOnLevel;
        private EffectHandler idleEffectHandler;

        #endregion




        #region Ctor

        public ResultPetCompleteBehaviour(Data _data, ILevelEnvironment _levelEnvironment)
        {
            data = _data;
            levelEnvironment = _levelEnvironment;

            animationPlayer = new SpineAnimationSequencePlayer();
        }

        #endregion



        #region Methods

        public void Initialize()
        {
            bool wasPetCollectedOnLevel = GameServices.Instance.LevelControllerService.LevelPetCollector.IsAnyPetCollected;
            bool wasPetInvokedOnLevel = levelEnvironment.Progress.WasPetInvoked;

            wasPetUsedOnLevel = wasPetInvokedOnLevel || wasPetCollectedOnLevel;

            CommonUtility.SetObjectActive(data.mainRoot, wasPetUsedOnLevel);

            if (wasPetInvokedOnLevel)
            {
                SetPetSkin(levelEnvironment.Progress.InvokedPetSkinType);          
            }
            else if(wasPetCollectedOnLevel)
            {
                SetPetSkin(GameServices.Instance.LevelControllerService.LevelPetCollector.LastCollectedPet);
            }


            void SetPetSkin(PetSkinType petSkinType)
            {
                SkeletonDataAsset skeletonDataAsset = IngameData.Settings.pets.uiSettings.FindMainMenuSkeletonData(petSkinType);

                data.sequencePlayerData.skeletonGraphic.skeletonDataAsset = skeletonDataAsset;
                data.sequencePlayerData.skeletonGraphic.Initialize(true);
            }
        }


        public void Deinitialize()
        {
            EffectManager.Instance.ReturnHandlerToPool(idleEffectHandler);
        }


        public void PlayAnimation()
        {
            if (!wasPetUsedOnLevel)
            {
                return;
            }

            animationPlayer.Play(data.sequencePlayerData, shouldLoopEnd: true);

            idleEffectHandler = EffectManager.Instance.CreateSystem(EffectKeys.FxGUIResultEmojiSpawn, true, default, default, data.fxRoot, TransformMode.Local);

            if (idleEffectHandler != null &&
                idleEffectHandler.TryGetComponent(out ResultPetCompleteFx resultFx))
            {
                // TODO: temp. Fix it later. Vladislav.k
                //resultFx.RandomFrameOverTime();
                resultFx.Play();
            }
        }

        #endregion
    }
}
