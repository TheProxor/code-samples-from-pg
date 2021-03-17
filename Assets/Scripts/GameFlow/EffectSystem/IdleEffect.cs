using System;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Effects
{
    [Serializable]
    public class IdleEffect 
    {
        #region Fields

        [SerializeField] private bool shouldShow = default;

        [EnableIf("shouldShow")]
        [Enum(typeof(EffectKeys))]
        [SerializeField] private string idleEffectName = default;

        [EnableIf("shouldShow")]
        [SerializeField] private Transform root = default;

        [EnableIf("shouldShow")]
        [SerializeField] private bool shouldOverrideLoops = default;

        [SerializeField] private Vector3 localScale = Vector3.one;

        private EffectHandler handler;

        private int additionalOrder;

        #endregion



        #region Properties

        public bool IsCreated => handler != null;

        public Transform Root => root;

        #endregion



        #region Methods

        public EffectHandler CreateAndPlayEffect(int ordersToAdd = 0)
        {
            if (shouldShow &&
                handler == null)
            {
                additionalOrder = ordersToAdd;

                handler = EffectManager.Instance.CreateSystem(idleEffectName,
                                                              true,
                                                              Vector3.zero,
                                                              Quaternion.identity,
                                                              root,
                                                              TransformMode.Local,
                                                              shouldOverrideLoops);

                if (handler != null)
                {
                    handler.transform.localScale = localScale;
                    handler.AddSortingOrder(additionalOrder);

                    handler.Clear();
                    handler.Play();
                }
            }

            return handler;
        }

        public void SetFxKey(string key) =>
            idleEffectName = key;


        public bool IsKeyEquals(string anotherKey) =>
             string.Equals(anotherKey, idleEffectName, StringComparison.Ordinal);


        public void SetTransformRoot(Transform _root) =>
            root = _root;


        public void StopEffect()
        {
            // TODO may remove it, because we reset all orders after pushing
            //handler.RemoveSortingOrder(additionalOrder);

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
            if (!handler.IsNull())
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


        public void SetParent(Transform parent)
        {
            if (handler != null)
            {
                handler.transform.SetParent(parent);
            }
        }

        #endregion
    }
}
