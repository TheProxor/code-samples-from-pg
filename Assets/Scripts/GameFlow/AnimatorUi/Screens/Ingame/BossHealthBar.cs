using System;
using UnityEngine;
using DG.Tweening;


namespace Drawmasters.Levels
{
    public class BossHealthBar : MonoBehaviour
    {
        #region Fields

        [SerializeField] private SpriteMask barFillMask = default;

        [SerializeField] private FactorAnimation healthBarAnimation = default;

        [SerializeField] private SpriteRenderer[] renderers = default;
        [SerializeField] private FactorAnimation healthBarFadeAnimation = default;
        [SerializeField] private FactorAnimation healthBarAppearAnimation = default;

        private EnemyBossBase enemyBoss;

        #endregion



        #region Methods

        public void Initialize(EnemyBossBase _enemyBoss)
        {
            barFillMask.transform.localScale = Vector3.one;
            SetRenderersAlpha(default);

            enemyBoss = _enemyBoss;

            StageLevelTargetComponent.OnShouldChangeStage += StageLevelTargetComponent_OnShouldChangeStage;
            enemyBoss.OnAppeared += EnemyBoss_OnAppeared;
            enemyBoss.OnDefeated += EnemyBoss_OnDefeated;
        }


        public void Deinitialize()
        {
            DOTween.Kill(this, true);

            StageLevelTargetComponent.OnShouldChangeStage -= StageLevelTargetComponent_OnShouldChangeStage;
            enemyBoss.OnAppeared -= EnemyBoss_OnAppeared;
            enemyBoss.OnDefeated -= EnemyBoss_OnDefeated;
        }


        private void SetRenderersAlpha(float value)
        {
            foreach (var spriteRenderer in renderers)
            {
                spriteRenderer.color = spriteRenderer.color.SetA(value);
            }
        }

        #endregion



        #region Events handlers

        private void PlayOnHitAnimation(float endFillValue, Action callback = null)
        {
            DOTween.Complete(this, true);

            float startFillValue = barFillMask.transform.localScale.x;

            healthBarAnimation.Play(value => barFillMask.transform.localScale = barFillMask.transform.localScale.SetX(Mathf.LerpUnclamped(startFillValue, endFillValue, value)),
                                    this,
                                    () =>
                                    {
                                        barFillMask.transform.localScale = barFillMask.transform.localScale.SetX(endFillValue);
                                        callback?.Invoke();
                                    });

        }

        private void StageLevelTargetComponent_OnShouldChangeStage(int stage, LevelTarget levelTarget)
        {
            float endFillValue = 1.0f - ((float)stage / LevelStageController.StagesCount);
            PlayOnHitAnimation(endFillValue);
        }


        private void EnemyBoss_OnAppeared()
        {
            enemyBoss.OnAppeared -= EnemyBoss_OnAppeared;

            healthBarAppearAnimation.Play(value => SetRenderersAlpha(value), this);
        }


        private void EnemyBoss_OnDefeated(LevelTarget defeatedTarget)
        {
            if (defeatedTarget == enemyBoss)
            {
                PlayOnHitAnimation(default, () => healthBarFadeAnimation.Play((value) => SetRenderersAlpha(value), this));
            }
        }

        #endregion
    }
}

