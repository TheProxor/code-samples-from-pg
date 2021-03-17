using DG.Tweening;
using Drawmasters.Effects;
using Drawmasters.Ui;
using Drawmasters.Utils;
using Modules.General;
using UnityEngine;


namespace Drawmasters
{
    public class SoulTrail : IDeinitializable
    {
        private readonly IngameScreen screen;
        private readonly VectorAnimation trailAnimation;
        private readonly Transform parent;

        public bool IsDone { get; private set; }
            
        #region Ctor

        public SoulTrail(IngameScreen _screen, Transform _parent)
        {
            screen = _screen;
            parent = _parent;
            trailAnimation = IngameData.Settings.pets.levelSettings.soulTrailAnimation;
            IsDone = false;
            
            PlayTrailFx();
        }

        #endregion
        
        
        
        #region IDeinitializable
        
        public void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(screen.transform);
        }
        
        #endregion



        #region ICurrencyRewardTrail
        
        private void PlayTrailFx()
        {
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIIngameEnemySoulAppear, parent.position, Quaternion.identity, parent);
            
            Vector3 startPosition = TransformUtility.WorldToUiPosition(parent.position);
            var handler = EffectManager.Instance.CreateSystem(EffectKeys.FxGUIIngameEnemySoulFly, 
                true, 
                startPosition,
                parent: screen.transform);

            float callbackDelay = trailAnimation.delay + trailAnimation.duration;

            if (handler != null)
            {
                int fxSortingOrder = screen.SortingOrder + (ViewManager.OrderOffset - 1);
                handler.SetSortingOrder(fxSortingOrder);

                Scheduler.Instance.CallMethodWithDelay(this, 
                    () => EffectManager.Instance.ReturnHandlerToPool(handler), 
                    callbackDelay + 1f); //additionalTrailHideDelay = 1f

                handler.transform.position = startPosition;
                handler.transform.localScale = Vector3.one;
                handler.Play();

                trailAnimation.SetupBeginValue(startPosition);

                Vector3 finishPosition = screen.PetStartPosition;
                
                trailAnimation.SetupEndValue(finishPosition);
                trailAnimation.Play(value =>
                    {
                        handler.transform.position = value;
                    }, 
                    screen.transform, () => IsDone = true);
            }
            else
            {
                Scheduler.Instance.CallMethodWithDelay(this, () => IsDone = true, callbackDelay);
            }
        }
        
        #endregion
    }
}