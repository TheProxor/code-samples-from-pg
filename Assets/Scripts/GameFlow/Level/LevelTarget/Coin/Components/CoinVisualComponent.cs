using DG.Tweening;
using Drawmasters.Effects;
using Modules.Sound;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class CoinVisualComponent : LevelObjectComponentTemplate<CurrencyLevelObject>
    {
        #region Fields

        private CoinLevelObjectSettings settings;
        private MeshRenderer mainRenderer;
        private MeshFilter mainMeshFilter;

        #endregion



        #region Ctor

        public CoinVisualComponent(MeshRenderer _meshRenderer, MeshFilter _mainMeshFilter)
        {
            mainRenderer = _meshRenderer;
            mainMeshFilter = _mainMeshFilter;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            settings = IngameData.Settings.coinLevelObjectSettings;

            SwitchModel();

            CoinCollectComponent.OnShouldCollectCoin += CoinCollectComponent_OnShouldCollectCoin;

            Vector3 targetRotation = levelObject.transform.eulerAngles;
            targetRotation.y += 360.0f;

            levelObject.transform
                .DORotate(targetRotation, 
                    settings.idleRotateDuration, RotateMode.FastBeyond360)
                .SetId(this)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);
        }


        public override void Disable()
        {
            DOTween.Kill(this);

            CoinCollectComponent.OnShouldCollectCoin -= CoinCollectComponent_OnShouldCollectCoin;

            SaveRotation();
        }

        private void SwitchModel()
        {
            mainMeshFilter.mesh = settings.FindMesh(levelObject.CurrencyType);
            mainRenderer.material = settings.FindMaterial(levelObject.CurrencyType);
        }


        private void SaveRotation()
        {
            if (!levelObject.CurrentData.IsNull())
            {
                levelObject.CurrentData.rotation = levelObject.transform.eulerAngles;
            }
        }

        #endregion



        #region Events handlers

        private void CoinCollectComponent_OnShouldCollectCoin(CurrencyLevelObject anotherCoin, 
            ICoinCollector collector)
        {
            if (levelObject == anotherCoin)
            {
                settings.scaleCollectAnimation.Play(
                        value => levelObject.transform.localScale = value, 
                        this,
                        () => levelObject.FinishGame());

                EffectManager.Instance.PlaySystemOnce(settings.fxKeyOnCollect,
                                         levelObject.transform.position,
                                         levelObject.transform.rotation);
                
                SoundManager.Instance.PlaySound(SoundGroupKeys.RandomCoinClaimKey);
            }
        }


        #endregion
    }
}
