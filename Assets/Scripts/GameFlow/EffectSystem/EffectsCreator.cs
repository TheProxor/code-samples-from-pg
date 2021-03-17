using UnityEngine;


namespace Drawmasters.Effects
{
    public class EffectsCreator : MonoBehaviour
    {
        #region Fields

        [Enum(typeof(EffectKeys))]
        [SerializeField] private string effectName = default;
        [SerializeField] private bool isLooped = default;
        [SerializeField] private bool isCreateOnAwake = true;
        [SerializeField] private Vector3 targetLocalScale = Vector3.one;

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            if (isCreateOnAwake)
            {
                CreateEffect();
            }
        }


        private void Start()
        {
            if (!isCreateOnAwake)
            {
                CreateEffect();
            }
        }

        #endregion



        #region Methods

        private void CreateEffect()
        {
            if (EffectManager.InstanceIfExist)
            {
                var effectHandler = EffectManager.Instance.CreateSystem(effectName,
                                                                        isLooped,
                                                                        parent: transform,
                                                                        transformMode: TransformMode.Local);
                if (effectHandler != null)
                {
                    effectHandler.transform.localScale = targetLocalScale;
                }
            }
        }

        #endregion
    }
}
