using Modules.InAppPurchase;
using Drawmasters.Effects;
using Modules.General;
using Modules.General.InAppPurchase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace Drawmasters.Ui
{
    public abstract class TransitionShopCell : BaseShopCell
    {
        #region Fields

        [SerializeField] private float becomeUnavailableDelay = default;
        [SerializeField] [Required] private Button transitionButton = default;

        #endregion



        #region IShopMenuCell

        public override void Initialize(StoreItem _storeItem)
        {
            base.Initialize(_storeItem);

            transitionButton.onClick.AddListener(MakeTransitional);
        }


        public override void Deinitialize()
        {
            transitionButton.onClick.RemoveListener(MakeTransitional);

            base.Deinitialize();
        }

        #endregion



        #region Methods

        protected abstract void OnShouldMakeTransitional(Action onPurchased);

        #endregion



        #region Events handlers

        private void MakeTransitional() =>
            OnShouldMakeTransitional(OnItemBought);
        

        private void OnItemBought()
        {
            EffectManager.Instance.PlaySystemOnce(BuyFxKey, parent: transform, transformMode: TransformMode.Local);

            InvokeBoughtEvent();

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                RefreshVisual();

                if (!storeItem.IsConsumable)
                {
                    InvokeBecomeUnavailableEvent();
                }
            }, becomeUnavailableDelay);
        }

        #endregion
    }
}
