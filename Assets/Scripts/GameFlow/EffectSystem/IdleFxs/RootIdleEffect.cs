using UnityEngine;


namespace Drawmasters.Effects
{
    public abstract class RootIdleEffect
    {
        #region Fields

        private string idleEffectName = default;

        protected Transform fxRoot;
        private EffectHandler handler;

        #endregion



        #region Properties

        public bool IsCreated => handler != null;

        #endregion



        #region Class lifecycle

        public RootIdleEffect(string _idleEffectName)
        {
            idleEffectName = _idleEffectName;
        }

        #endregion



        #region Methods

        public virtual EffectHandler CreateAndPlayEffect()
        {
            if (handler == null)
            {
                fxRoot = GetFxRoot();

                handler = EffectManager.Instance.CreateSystem(idleEffectName,
                                                              true,
                                                              Vector3.one,
                                                              Quaternion.identity,
                                                              fxRoot,
                                                              TransformMode.Local);

                if (handler != null)
                {
                    handler.Clear();
                    handler.Play();
                }
            }

            return handler;
        }


        public virtual void StopEffect()
        {
            EffectManager.Instance.ReturnHandlerToPool(handler);
            handler = null;
        }


        public void PauseEffect()
        {
            if (handler != null && !handler.IsPaused)
            {
                handler.Pause();
            }
        }


        public void SetAlpha(float value)
        {
            if (handler != null)
            {
                handler.Color = handler.Color.SetA(value);
            }
        }


        public void UpdateSortingOrder(int order = 0)
        {
            if (handler != null)
            {
                handler.SetSortingOrder(order);
            }
        }


        public void SetupFxKey(string fxKey) =>
            idleEffectName = fxKey;


        protected abstract Transform GetFxRoot();

        #endregion
    }
}
