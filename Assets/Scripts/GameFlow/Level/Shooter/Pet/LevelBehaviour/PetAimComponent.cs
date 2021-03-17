using System;
using DG.Tweening;
using Drawmasters.Levels;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Pets
{
    public class PetAimComponent : PetComponent
    {
        #region Fields
         
        public static event Action<Pet, LevelObject> OnAimed;

        private readonly PetLevelSettings petLevelSettings;

        private LineRenderer pathRenderer;
        private GameObject scopeObject;

        private LevelObject aimingLevelObject;

        #endregion



        #region Class lifecycle

        public PetAimComponent()
        {
            petLevelSettings = IngameData.Settings.pets.levelSettings;
        }

        #endregion



        #region Methods

        public override void Initialize(Pet _pet)
        {
            base.Initialize(_pet);

            PetInvokeComponent.OnShouldInvokePetForLevel += PetInvokeComponent_OnShouldInvokePetForLevel;
        }


        public override void Deinitialize()
        {
            PetInvokeComponent.OnShouldInvokePetForLevel -= PetInvokeComponent_OnShouldInvokePetForLevel;

            PetShootComponent.OnShouldPrepareShot -= PetShootComponent_OnShouldPrepareShoot;
            PetShootComponent.OnShouldDiscardShot -= PetShootComponent_OnShouldDiscardShot;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(this);

            aimingLevelObject = null;

            if (pathRenderer != null)
            {
                Content.Management.DestroyObject(pathRenderer.gameObject);
            }

            pathRenderer = null;

            if (scopeObject != null)
            {
                Content.Management.DestroyObject(scopeObject);
            }

            scopeObject = null;

            base.Deinitialize();
        }


        private void StartAiming(LevelObject levelObject)
        {
            aimingLevelObject = levelObject;

            Clear();

            CommonUtility.SetObjectActive(scopeObject, true);

            petLevelSettings.aimScopeScaleAnimation.Play((value) =>
            {
                scopeObject.transform.position = levelObject.CenterPosition;
                scopeObject.transform.localScale = value;
            }, this);

            RefreshLineRenderer(aimingLevelObject);
            petLevelSettings.aimLineAnimation.Play((value) =>
            {
                pathRenderer.widthMultiplier = value;
                RefreshLineRenderer(aimingLevelObject);
            }, this);
        }


        private void StopAiming()
        {
            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleMethod(this, OnShotReady);
            Clear();

            aimingLevelObject = null;
        }


        private void RefreshLineRenderer(LevelObject levelObject)
        {
            Vector3[] pathTrajectory = new Vector3[] { mainPet.CurrentSkinLink.CurrentPosition,
                                                       levelObject.CenterPosition };

            pathRenderer.positionCount = pathTrajectory.Length;
            pathRenderer.SetPositions(pathTrajectory);
        }


        private void Clear()
        {
            pathRenderer.positionCount = 0;
            pathRenderer.SetPositions(Array.Empty<Vector3>());

            CommonUtility.SetObjectActive(scopeObject, false);
        }

        #endregion



        #region Events handlers

        private void PetInvokeComponent_OnShouldInvokePetForLevel(Pet pet)
        {
            if (pet != mainPet)
            {
                return;
            }

            pathRenderer = Content.Management.CreateDefaultLineRenderer(mainPet.CurrentSkinLink.transform);
            pathRenderer.sortingOrder = mainPet.CurrentSkinLink.MeshRenderer.sortingOrder - 1;
            pathRenderer.sortingLayerID = mainPet.CurrentSkinLink.MeshRenderer.sortingLayerID;
            pathRenderer.colorGradient = petLevelSettings.FindAimGradient(mainPet.Type);

            scopeObject = Content.Management.Create(petLevelSettings.aimScopePrefab, null);

            Clear();

            PetShootComponent.OnShouldPrepareShot += PetShootComponent_OnShouldPrepareShoot;
            PetShootComponent.OnShouldDiscardShot += PetShootComponent_OnShouldDiscardShot;
        }


        private void PetShootComponent_OnShouldPrepareShoot(Pet anotherPet, LevelObject levelObject)
        {
            if (mainPet != anotherPet)
            {
                return;
            }

            StartAiming(levelObject);

            Scheduler.Instance.CallMethodWithDelay(this, OnShotReady, petLevelSettings.aimDuration);
        }


        private void PetShootComponent_OnShouldDiscardShot(Pet anotherPet, LevelObject levelObject)
        {
            if (mainPet != anotherPet && aimingLevelObject != levelObject)
            {
                return;
            }

            StopAiming();
        }


        private void OnShotReady()
        {
            if (aimingLevelObject == null)
            {
                CustomDebug.Log($"##### aimingLevelObject is NULL"); // Temp log pattern to check for devices
                StopAiming();
                return;
            }

            OnAimed?.Invoke(mainPet, aimingLevelObject);

            StopAiming();
        }

        #endregion
    }
}
