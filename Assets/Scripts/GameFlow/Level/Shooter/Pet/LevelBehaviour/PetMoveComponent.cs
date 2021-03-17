using System;
using System.Collections;
using Drawmasters.Levels;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Pets
{
    public class PetMoveComponent : PetComponent
    {
        #region Fields

        public static event Action<Pet> OnMoveStarted;
        public static event Action<Pet, PetSkinType> OnShouldInvokePetForLevel;

        public static event Action<Pet, PetLevelSettings.MoveType> OnMoveTypeChanged;

        private PetLevelSettings petLevelSettings;
        private PetLevelSettings.Data petLevelSettingsData;

        private Rect gameZoneRect;

        private float currentMoveDuration;
        private Vector2 currentDirection;

        private Coroutine slowUpRoutine;
        private PetLevelSettings.MoveType currentMoveType;

        private bool shouldLockMoveScale;
        private LevelObject lockedAimingLevelObject;

        #endregion



        #region Methods

        public override void Initialize(Pet _pet)
        {
            base.Initialize(_pet);

            petLevelSettings = IngameData.Settings.pets.levelSettings;
            petLevelSettingsData = petLevelSettings.FindData(mainPet.Type);

            Camera gameCamera = IngameCamera.Instance.Camera;
            gameZoneRect = CommonUtility.CalculateGameZoneRect(gameCamera);

            PetInvokeComponent.OnShouldInvokePetForLevel += PetInvokeComponent_OnShouldInvokePetForLevel;

            PetShootComponent.OnShouldPrepareShot += PetShootComponent_OnShouldPrepareShot;
            PetShootComponent.OnShouldDiscardShot += PetShootComponent_OnShouldDiscardShot;
            PetShootComponent.OnShooted += PetShootComponent_OnShooted;
        }


        public override void Deinitialize()
        {
            PetInvokeComponent.OnShouldInvokePetForLevel -= PetInvokeComponent_OnShouldInvokePetForLevel;
            PetShootComponent.OnShooted -= PetShootComponent_OnShooted;
            PetShootComponent.OnShouldPrepareShot -= PetShootComponent_OnShouldPrepareShot;
            PetShootComponent.OnShouldDiscardShot -= PetShootComponent_OnShouldDiscardShot;

            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            MonoBehaviourLifecycle.OnUpdate -= OnUpdateVelocity;
            MonoBehaviourLifecycle.OnUpdate -= OnShouldAim;

            MonoBehaviourLifecycle.StopPlayingCorotine(slowUpRoutine);

            StopMonitorMoves();
            currentMoveDuration = default;

            base.Deinitialize();
        }


        private void StartMonitorMoves()
        {
            mainPet.Input.OnStartDraw += Input_OnStartDraw;
            mainPet.Input.OnDraw += Input_OnDraw;
            mainPet.Input.OnDrawFinish += Input_OnDrawFinish;
        }


        private void StopMonitorMoves()
        {
            if (mainPet.Input != null)
            {
                mainPet.Input.OnStartDraw -= Input_OnStartDraw;
                mainPet.Input.OnDraw -= Input_OnDraw;
                mainPet.Input.OnDrawFinish -= Input_OnDrawFinish;
            }
        }


        private bool IsInGameZone(Vector3 currentPosition)
        {
            Vector3 petRectPosition = currentPosition + petLevelSettingsData.rectOffset;
            float petRectWidth = petLevelSettingsData.rectWidth;
            float petRectheight = petLevelSettingsData.rectHeight;

            bool result = gameZoneRect.Contains(petRectPosition.SetX(petRectPosition.x + petRectWidth)) &&
                          gameZoneRect.Contains(petRectPosition.SetX(petRectPosition.x - petRectWidth)) &&
                          gameZoneRect.Contains(petRectPosition.SetY(petRectPosition.y + petRectheight)) &&
                          gameZoneRect.Contains(petRectPosition.SetY(petRectPosition.y - petRectheight));
            return result;
        }


        private void SetPetScaleAim(LevelObject levelObjet)
        {
            if (lockedAimingLevelObject != null)
            {
                float scaleSingX = mainPet.CurrentSkinLink.CurrentPosition.x < levelObjet.transform.position.x ? 1.0f : -1.0f;
                SetPetScale(scaleSingX);
            }
        }


        private void SetPetScale(float scaleSingX)
        {
            float scaleX = Mathf.Abs(mainPet.CurrentSkinLink.transform.localScale.x) * scaleSingX;
            mainPet.CurrentSkinLink.transform.localScale = mainPet.CurrentSkinLink.transform.localScale.SetX(scaleX);
        }

        #endregion



        #region Events handlers

        private void PetInvokeComponent_OnShouldInvokePetForLevel(Pet pet)
        {
            if (pet == mainPet)
            {
                // TODO: Find out why gameZoneRect.center gives us wrong position (somewhere behind right screen's corner) on devices
                // Using concrete default position as hack
                mainPet.CurrentSkinLink.transform.position = Vector3.zero;

                StartMonitorMoves();

                MonoBehaviourLifecycle.OnUpdate += OnUpdateVelocity;
                Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            }
        }


        private void Input_OnStartDraw(Vector3 touchPosition)
        {
            MonoBehaviourLifecycle.StopPlayingCorotine(slowUpRoutine);

            OnMoveStarted?.Invoke(mainPet);
        }


        private void Input_OnDraw(Vector2 direction, float deltaTime)
        {
            currentDirection = direction;

            currentMoveDuration += deltaTime;

            if (!shouldLockMoveScale)
            {
                float scaleSingX = direction.x < 0 ? -1.0f : 1.0f;
                SetPetScale(scaleSingX);
            }
        }


        private void Input_OnDrawFinish(bool success, Vector3 position)
        {
            slowUpRoutine = MonoBehaviourLifecycle.PlayCoroutine(SlowUp());

            IEnumerator SlowUp()
            {
                float savedBegin = currentMoveDuration;
                float savedEnd = 0.0f;

                float routineTime = 0.0f;

                while (routineTime < petLevelSettings.slowUpDuration)
                {
                    currentMoveDuration = savedBegin + petLevelSettings.slowUpAnimationCurve.Evaluate(routineTime / petLevelSettings.slowUpDuration) * (savedEnd - savedBegin);
                    routineTime += Time.deltaTime;

                    yield return null;
                }

                currentMoveDuration = default;
            }
        }


        private void OnUpdateVelocity(float deltaTime)
        {
            float maxMoveVelocityDurationFactor = Mathf.Clamp01(currentMoveDuration / petLevelSettings.maxMoveVelocityDuration);
            float velocityFactor = petLevelSettings.moveAnimationCurve.Evaluate(maxMoveVelocityDurationFactor);
            float velocity = petLevelSettings.baseMoveVelocity + petLevelSettings.maxMoveVelocity * velocityFactor;

            Vector3 offset = currentDirection.normalized * velocity * deltaTime;
            Vector3 resultPosition = mainPet.CurrentSkinLink.transform.position + offset;

            if (IsInGameZone(resultPosition))
            {
                mainPet.CurrentSkinLink.transform.Translate(currentDirection.normalized * velocity * deltaTime);
            }

            PetLevelSettings.MoveType moveType = petLevelSettings.GetMoveType(velocity);
            if (currentMoveType != moveType)
            {
                currentMoveType = moveType;
                OnMoveTypeChanged?.Invoke(mainPet, currentMoveType);
            }
        }


        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.AllTargetsHitted)
            {
                StopMonitorMoves();
                Input_OnDrawFinish(true, default);
            }
        }


        private void PetShootComponent_OnShouldDiscardShot(Pet anotherPet, LevelObject arg2) =>
            PetShootComponent_OnShooted(anotherPet, arg2.transform.position);


        private void PetShootComponent_OnShooted(Pet anotherPet, Vector3 targetPosition)
        {
            if (mainPet != anotherPet)
            {
                return;
            }

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                shouldLockMoveScale = false;
                MonoBehaviourLifecycle.OnUpdate -= OnShouldAim;

                lockedAimingLevelObject = null;
            }, petLevelSettings.lockScaleOnAttackDuration);
        }


        private void PetShootComponent_OnShouldPrepareShot(Pet anotherPet, LevelObject levelObjet)
        {
            if (mainPet != anotherPet)
            {
                return;
            }

            lockedAimingLevelObject = levelObjet;

            SetPetScaleAim(levelObjet);
            shouldLockMoveScale = true;

            MonoBehaviourLifecycle.OnUpdate += OnShouldAim;
        }


        private void OnShouldAim(float deltaTime) =>
            SetPetScaleAim(lockedAimingLevelObject);
        
        #endregion
    }
}
