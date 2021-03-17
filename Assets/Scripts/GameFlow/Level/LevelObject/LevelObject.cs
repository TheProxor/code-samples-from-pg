using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using Modules.General;


namespace Drawmasters.Levels
{
    public class LevelObject : SerializedMonoBehaviour, IStartGame, IFinishGame, IMovable
    {
        #region Fields

        public event Action<List<LevelObject>> OnLinksSet;
        public event Action<LevelObject> OnGameFinished;

        public event Action OnPrepareForCome;
        public event Action OnStageCame;

        [Header("LevelObject")]
        [SerializeField] private int index = -1;
        [Space]
        [SerializeField] private bool isStaticByDefault = default;

        private RigidbodyType2D defaultType;
        private LevelObjectMoveSettings moveSettings;

        private Collider2D[] objectColliders;
        private Renderer[] renderers;

        private bool isFreeFalling;

        protected StageLevelObjectData currentStageData;

        private Coroutine allowTeleportRoutine;


        #endregion



        #region Properties

        public int Index => index;

        public bool IsStaticByDefault => isStaticByDefault;

        public virtual Vector3 CenterPosition =>
            transform.position;

        public LevelObjectData CurrentData { get; private set; }

        public GameMode CurrentGameMode { get; private set; }

        public WeaponType WeaponType { get; private set; }

        public Rigidbody2D Rigidbody2D { get; set; }

        public bool IsPrepareForCome { get; private set; }

        protected bool CanMove => moveSettings.CanMove;

        protected Renderer[] Renderers =>
            renderers = renderers ?? GetComponentsInChildren<Renderer>();


        private Collider2D[] ObjectColliders =>
            objectColliders = objectColliders ?? GetComponentsInChildren<Collider2D>();


        public virtual bool IsHardReturnToInitialState => true;

        public virtual bool AllowToTeleport { get; private set; } = true;

        public WaitForSeconds WaitForAllowTeleport { get; private set; }

        #endregion



        #region Unity Lifecycle

        protected virtual void Awake() =>
            GetPhysicalBody();


        private void OnDisable()
        {
            StopMoving();
            ResetRigidbodyMovement();

            IsPrepareForCome = false;
            isFreeFalling = false;

            DOTween.Kill(this);

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }

        #endregion



        #region Methods

        public virtual void StartMoving()
        {
            if (moveSettings.CanMove)
            {
                float totalPathDistance = moveSettings.TotalPathDistance;

                Vector3 firstPathPoint = moveSettings.path.FirstObject();
                Rigidbody2D.position = firstPathPoint;

                Sequence moveSequence = DOTween.Sequence()
                                               .SetTarget(Rigidbody2D)
                                               .SetLoops(-1, moveSettings.isCycled ? LoopType.Restart : LoopType.Yoyo);

                for (int i = 0; i < moveSettings.path.Count; i++)
                {
                    float distance = i > 0 ? Vector2.Distance(moveSettings.path[i], moveSettings.path[i - 1]) : default;
                    float moveDuration = totalPathDistance > 0.0f ? moveSettings.totalMoveDuration * (distance / totalPathDistance) : default;

                    moveSequence
                     .Append(Rigidbody2D.DOMove(moveSettings.path[i], moveDuration).SetEase(Ease.Linear).SetTarget(Rigidbody2D))
                     .AppendInterval(moveSettings.delayBetweenPoints);
                }

                if (moveSettings.isCycled)
                {
                    float distance = Vector2.Distance(moveSettings.path.Last(), moveSettings.path.First());
                    float moveDuration = totalPathDistance > 0.0f ? moveSettings.totalMoveDuration * (distance / totalPathDistance) : default;

                    moveSequence
                     .Append(Rigidbody2D.DOMove(moveSettings.path.First(), moveDuration).SetEase(Ease.Linear).SetTarget(Rigidbody2D))
                     .AppendInterval(moveSettings.delayBetweenPoints);
                }
            }
        }


        public void StopMoving() =>
            DOTween.Kill(Rigidbody2D);


        public virtual void FreeFallObject(bool isImmediately)
        {
            StopMoving();

            float delay = isImmediately ? 0.0f : IngameData.Settings.bossLevelSettings.objectFreeFallDelay;

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                IsPrepareForCome = false;
                isFreeFalling = true;

                if (Rigidbody2D != null)
                {
                    SetRigidbodySimulated(true);
                    Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                    Rigidbody2D.gravityScale = IngameData.Settings.bossLevelSettings.objectFreeFallGravityScale;
                }

                SetCollidersEnabled(false);
                OnFreeFall();
            }, delay);
        }


        public void ComeObject(StageLevelObjectData data, bool isImmediately)
        {
            transform.eulerAngles = data.rotation;
            PlayChangePositionStageAnimation(data.position, data.comeVelocity, isImmediately);
        }


        public virtual void StartStageChange(StageLevelObjectData data, int stage)
        {
            if (stage == 0 && !IsPrepareForCome)
            {
                transform.position = data.position;
                transform.eulerAngles = data.rotation;
                moveSettings = data.moveSettings;
            }

            currentStageData = data;
        }


        public virtual void FinishStageChange(int stage)
        {
            DOTween.Complete(this, true);

            if (!IsPrepareForCome && !isFreeFalling)
            {
                SetRigidbodySimulated(true);
                SetCollidersEnabled(true);
            }

            if (stage == CurrentData.createdStageIndex)
            {
                OnStageCame?.Invoke();
            }
        }

        public virtual void ReturnToStage(StageLevelObjectData data, int stage)
        {
            currentStageData = data;

            if (CurrentData.createdStageIndex <= stage && !IsPrepareForCome)
            {
                transform.position = currentStageData.position;
                transform.eulerAngles = currentStageData.rotation;
                moveSettings = currentStageData.moveSettings;
            }

            DOTween.Complete(this, true);

            if (!IsPrepareForCome && !isFreeFalling)
            {
                SetRigidbodySimulated(true);
                SetCollidersEnabled(true);
            }

            ResetRigidbodyMovement();

            #warning hotfix. Sometimes velocity doesn't resets to zero
            if (CurrentData.createdStageIndex <= stage && !IsPrepareForCome && !isFreeFalling)
            {
                SetRigidbodySimulated(false);
                Scheduler.Instance.CallMethodWithDelay(this, () => SetRigidbodySimulated(true), CommonUtility.OneFrameDelay);
            }

            if (stage == CurrentData.createdStageIndex)
            {
                OnStageCame?.Invoke();
            }
        }


        private void PlayChangePositionStageAnimation(Vector3 targetPosition, float velocity, bool isImmediately)
        {
            float duration = isImmediately ? 0.0f : ComeDuration(targetPosition, velocity);

            transform
                .DOMove(targetPosition, duration)
                .SetEase(IngameData.Settings.bossLevelSettings.objectsComeCurve)
                .SetDelay(IngameData.Settings.bossLevelSettings.objectsComeDelay)
                .OnComplete(() =>
                {
                    IsPrepareForCome = false;
                })
                .SetId(this);
        }


        public virtual void SetData(LevelObjectData data)
        {
            if (IsPrepareForCome)
            {
                return;
            }

            transform.position = data.position;
            transform.eulerAngles = data.rotation;
            moveSettings = data.moveSettings;

            CurrentData = data;
        }

        public void ReturnToInitialState()
        {
            StartReturnToInitialState();
            PreSetData();
            SetData(CurrentData);

            OnShouldFinishReturnToInitialState();

            FinishReturnToInitialState();
        }

        protected virtual void OnShouldFinishReturnToInitialState() { }


        public virtual void SetLinks(List<LevelObject> linkedObjects)
        {
            OnLinksSet?.Invoke(linkedObjects);
        }


        public virtual void StartGame(GameMode mode, WeaponType _weaponType, Transform levelTransform)
        {
            if (IsPrepareForCome)
            {
                return;
            }

            CurrentGameMode = mode;
            WeaponType = _weaponType;

            FinishReturnToInitialState();
        }


        public virtual void FinishGame()
        {
            StartReturnToInitialState();
            OnGameFinished?.Invoke(this);
        }


        public bool EqualData(LevelObjectData data) => CurrentData == data;

        protected virtual void GetPhysicalBody()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();

            if (Rigidbody2D != null)
            {
                defaultType = Rigidbody2D.bodyType;
            }
        }

        public virtual void PreSetData() { }


        public virtual void PrepareForAppear(StageLevelObjectData data)
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(this);

            transform.eulerAngles = data.rotation;
            moveSettings = data.moveSettings;
            transform.position = data.position;

            StopMoving();

            SetRigidbodySimulated(false);

            SetupMaxGameZonePositionY();

            IsPrepareForCome = true;

            OnPrepareForCome?.Invoke();
        }


        public void SetupMaxGameZonePositionY()
        {
            float gameZoneRectMultiplier = IngameData.Settings.bossLevelSettings.gameZoneMultiplierForObjectsPrepare;
            Rect gameZoneRect = CommonUtility.CalculateGameZoneRect(IngameCamera.Instance.Camera, gameZoneRectMultiplier);

            transform.position = transform.position.SetY(gameZoneRect.yMax);
        }


        public float ComeDuration(Vector3 targetPosition, float velocity)
        {
            velocity = velocity <= 0 ? IngameData.Settings.bossLevelSettings.objectsComeVelocity : velocity;

            float distance = Vector3.Distance(transform.position, targetPosition);
            return distance / velocity;
        }


        protected virtual void OnFreeFall() { }

        private void SetCollidersEnabled(bool enable)
        {
            foreach (var c in ObjectColliders)
            {
                c.enabled = enable;
            }
        }


        private void SetRigidbodySimulated(bool simulated)
        {
            if (Rigidbody2D != null && !IsPrepareForCome)
            {
                Rigidbody2D.simulated = simulated;
            }
        }


        public void MarkTeleported()
        {
            AllowToTeleport = false;

            allowTeleportRoutine = MonoBehaviourLifecycle.PlayCoroutine(AllowTeleportation());

            IEnumerator AllowTeleportation()
            {
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                yield return WaitForAllowTeleport;

                AllowToTeleport = true;
            }
        }


        protected virtual void FinishReturnToInitialState()
        {
            SetRigidbodySimulated(true);
            if (Rigidbody2D != null)
            {
                Rigidbody2D.bodyType = moveSettings.CanMove ? RigidbodyType2D.Kinematic : defaultType;
                Rigidbody2D.Sleep();
            }

            ResetRigidbodyMovement();
            SetCollidersEnabled(true);
            AllowToTeleport = true;
            WaitForAllowTeleport = new WaitForSeconds(IngameData.Settings.portalsSettings.delayForNextTeleportation);
        }


        protected virtual void StartReturnToInitialState()
        {
            StopMoving();

            IsPrepareForCome = false;
            isFreeFalling = false;

            if (Rigidbody2D != null)
            {
                ResetRigidbodyMovement();

                SetRigidbodySimulated(false);
                Rigidbody2D.Sleep();
            }

            MonoBehaviourLifecycle.StopPlayingCorotine(allowTeleportRoutine);
        }


        private void ResetRigidbodyMovement()
        {
            if (Rigidbody2D != null && Rigidbody2D.bodyType != RigidbodyType2D.Static)
            {
                Rigidbody2D.velocity = Vector3.zero;
                Rigidbody2D.angularVelocity = 0.0f;
            }
        }

        #endregion
    }
}
