using Modules.Sound;
using Drawmasters.Effects;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class DestroyVfxComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        [Enum(typeof(EffectKeys))]
        [SerializeField] private string effectName = default;

        [Enum(typeof(AudioKeys.Ingame))]
        [SerializeField] private string[] soundKeys = default;

        private EffectHandler effectHandler;

        #endregion



        #region Methods

        public override void Enable()
        {
            if (sourceLevelObject != null)
            {
                sourceLevelObject.OnPreDestroy += SourceLevelObject_OnBreak;
            }            
        }


        public override void Disable()
        {
            if (sourceLevelObject != null)
            {
                sourceLevelObject.OnPreDestroy -= SourceLevelObject_OnBreak;
            }
        }

        #endregion



        #region Events handlers

        void SourceLevelObject_OnBreak(PhysicalLevelObject levelObject)
        {
            if (effectHandler != null && !effectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(effectHandler);
            }

            effectHandler = EffectManager.Instance.CreateSystem(effectName,
                                                                false, // no matter because of last argument
                                                                sourceLevelObject.transform.position,
                                                                sourceLevelObject.transform.rotation,
                                                                sourceLevelObject.transform.parent,
                                                                TransformMode.World,
                                                                false);
            if (effectHandler != null)
            {
                effectHandler.Play();
            }

            if (soundKeys != null && soundKeys.Length > 0)
            {
                SoundManager.Instance.PlaySound(soundKeys.RandomObject());
            }
        }

        #endregion
    }
}
