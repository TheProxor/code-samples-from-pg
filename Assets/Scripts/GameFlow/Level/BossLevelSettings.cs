using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "BossLevelSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "BossLevelSettings")]
    public class BossLevelSettings : ScriptableObject
    {
        #region Fields

        public float delayBetweenStages = default;

        [Header("Shooter")]
        public float additionalShooterAllowShootDelay = default;

        [Header("Free fall settings")]
        public float objectFreeFallGravityScale = default;
        public float objectFreeFallDelay = default;

        public float gameZoneMultiplierForObjectsPrepare = default;

        [Header("Animation Settings")]
        public float objectsComeVelocity = default;
        public float objectsComeDelay = default;
        public AnimationCurve objectsComeCurve = default;

        [Header("Win Settings")]
        public float objectVelocityForWinEffects = default;

        [Header("Effect Settings")]
        public float fadeOutBossEffectdelay = default;
        public float bossAppearEffectDelay = default;

        [Header("Sound")]
        public float soundGreetingDelay = default;

        #endregion
    }
}
