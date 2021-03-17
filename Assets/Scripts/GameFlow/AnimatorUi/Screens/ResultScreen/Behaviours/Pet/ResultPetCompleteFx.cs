using UnityEngine;


namespace Drawmasters.Effects
{
    public class ResultPetCompleteFx : MonoBehaviour
    {
        #region Fields

        [SerializeField] private EffectHandler effectHandler = default;
        [SerializeField] private ParticleSystem emodjiSystem = default;

        [SerializeField] private float minFrameOverTime = default;
        [SerializeField] private float maxFrameOverTime = default;


        #endregion



        #region Methods

        public void Play()
        {
            effectHandler.Play();
        }


        [Sirenix.OdinInspector.Button]
        public void RandomFrameOverTime()
        {
            float minFrameOverTimeToSet = Random.Range(0, minFrameOverTime);
            float maxFrameOverTimeToSet = Random.Range(minFrameOverTimeToSet, maxFrameOverTime);

            ParticleSystem.TextureSheetAnimationModule module = emodjiSystem.textureSheetAnimation;
            var frameOverTime = module.frameOverTime;
            module.startFrame = new ParticleSystem.MinMaxCurve(minFrameOverTimeToSet, maxFrameOverTimeToSet);

            frameOverTime.constantMin = minFrameOverTimeToSet;
            frameOverTime.constantMax = maxFrameOverTimeToSet;

            module.frameOverTime = frameOverTime;
        }


        #endregion
    }
}
