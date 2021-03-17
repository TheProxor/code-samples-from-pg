using UnityEngine;
using DG.Tweening;
using Drawmasters.Pool;
using Modules.General;

namespace Drawmasters.Levels
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Announcer : MonoBehaviour
    {
        #region Fields

        [SerializeField] private SpriteRenderer spriteRenderer = default;

        private ComponentPool pool;


        #endregion



        #region Properies
        
        public bool IsActive { get; private set; }

        private static Color UAColor { get; set; } = Color.white;

        #endregion



        #region Public methods

        public void Init(ComponentPool handledPool,
                         Sprite sprite,
                         Vector3 startPosition,
                         Vector3 finishPosition,
                         float lifeTime,
                         AnimationCurve moveCurve = null,
                         AnimationCurve alphaCurve = null,
                         AnimationCurve scaleCurve = null)
        {
            pool = handledPool;
            
            IsActive = true;

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = UAColor;
            transform.localScale = Vector3.one;

            transform.position = startPosition;

            if (moveCurve != null)
            {
                transform.DOMove(finishPosition, lifeTime)
                    .SetEase(moveCurve)
                    .SetId(this);
            }

            if (alphaCurve != null)
            {
                spriteRenderer.DOFade(0.0f, lifeTime)
                    .SetEase(alphaCurve)
                    .SetId(this);
            }

            if (scaleCurve != null)
            {
                transform.localScale = Vector3.zero;
                transform.DOScale(Vector3.one, lifeTime)
                    .SetEase(scaleCurve)
                    .SetId(this);
            }

            Scheduler.Instance.CallMethodWithDelay(this, Deinitialize, lifeTime);
        }


        public void Deinitialize()
        {
            IsActive = false;
            pool.Push(this);
            
            DOTween.Kill(this);
        }

        #endregion
    }
}
