using Drawmasters.Effects;
using Drawmasters.Proposal;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class UiRewardItem : MonoBehaviour
    {
        #region Fields

        [SerializeField] private RectTransform mainTransform = default;
        [SerializeField] private Transform vfxRoot = default;
        [SerializeField] private Canvas itemCanvas = default;

        protected RewardData rewardData;
        private EffectHandler effectHandler;
        #endregion



        #region Properties

        public RectTransform MainTransform => mainTransform;

        #endregion
        
        
        
        #region Public methods

        public virtual void InitializeUiRewardItem(RewardData _rewardData, int sortingOrder)
        {
            rewardData = _rewardData;

            itemCanvas.sortingLayerName = RenderLayers.Ui;
            itemCanvas.sortingOrder = sortingOrder + 5;

            if (vfxRoot != null)
            {
                PlayVfx(sortingOrder + 1);    
            }
        }

        
        public virtual  void DeinitializeUiRewardItem()
        {
            StopVfx();
        }

        #endregion



        #region Private methods

        private void PlayVfx(int sortingOrder)
        {
            effectHandler = EffectManager.Instance.CreateSystem(EffectKeys.FxGUIRewardChestShine,
                true,
                Vector3.zero,
                Quaternion.identity,
                vfxRoot,
                TransformMode.Local);

            if (effectHandler != null)
            {
                effectHandler.SetSortingOrder(sortingOrder);
                effectHandler.transform.localScale = Vector3.one;
            }
        }

        
        private void StopVfx()
        {
            if (effectHandler != null)
            {
                effectHandler.Stop();
            }
        }

        #endregion
    }
}