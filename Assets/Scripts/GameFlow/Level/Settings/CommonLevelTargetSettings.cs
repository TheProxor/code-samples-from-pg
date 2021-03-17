using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "CommonLevelTargetSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "CommonLevelTargetSettings")]
    public class CommonLevelTargetSettings : ScriptableObject
    {
        #region Fields

        [Header("Stand")]
        public float enemyMass = default;
        public float bossGravityScale = default;
        public float enemyGravityScale = default;

        [Tooltip("Масса рутовой кости рэгдолла")]
        public float ragdollRootMass = default;

        [Tooltip("Угловой лимит на вращение кости относительно родителя")]
        public float ragdollRotationLimit = default;

        [Header("Ragdoll")]
        public float physicalsObjectsImpulsToApplyRagdoll = default;
        public float maxRotationAngleToApplyRagdoll = default;

        public PhysicsMaterial2D ragdollMaterial = default;

        [Header("Damage")]
        public float limbPartDamageReceiveImpulsMultiplier = default;
        [Tooltip("Множитель урона от врага по объектам (не влияет на получение урона самому врага)")]
        public float physicalObjectDamageMultiplier = default;

        [Tooltip("Длительность горения в кислоте. Потом цель умирает")]
        public float corroseDuration = default;

        [Tooltip("Длительность в течение которого перс будет окрашиваться от кислоты")]
        public float greenColorSetDuration = default;

        [Tooltip("Длительность в течение которого после зеленения перс будет пропадать")]
        public float clearColorSetDuration = default;
        
        [Tooltip("Длительность горения под лазером. Потом цель умирает")]
        public float laserDestroyDuration = default;

        [Tooltip("Длительность в течение которого перс будет окрашиваться от лазера")]
        public float laserColorSetDuration = default;

        [Tooltip("Длительность в течение которого после зеленения перс будет пропадать от лазера")]
        public float laserClearColorSetDuration = default;

        [Tooltip("Задержка для проигрывания эффекта взрыва лазера")]
        public float laserFxExplodeDelay = default;


        [Header("Animation")]
        [Tooltip("Минимальное количество обычных айдлов, после которых проиграется специальный айдл")]
        public int minIdlesCountBeforeSpecialIdle = default;

        [Tooltip("Максимальное количество обычных айдлов, после которых проиграется специальный айдл")]
        public int maxIdlesCountBeforeSpecialIdle = default;

        [Header("Sound")]
        public float SimultaneousDeathDuration = default;
        public float SimultaneousDeathSoundDelay = default;

        public float limbDamageForCollisionSound = default;

        #endregion
    }
}
