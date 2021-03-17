using Drawmasters.Levels.Inerfaces;
using Drawmasters.Pool;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class LevelCurrencyAnnouncersController : ILevelController, IInitialStateReturn
    {
        #region Fields

        private readonly List<TMP_Text> announcers = new List<TMP_Text>();

        private readonly CoinLevelObjectSettings settings;

        #endregion



        #region Class lifecycle

        public LevelCurrencyAnnouncersController()
        {
            settings = IngameData.Settings.coinLevelObjectSettings;
        }

        #endregion



        #region Methods

        public void Initialize()
        {
            CoinCollectComponent.OnShouldCollectCoin += CoinCollectComponent_OnShouldCollectCoin;
            RewardCollectComponent.OnRewardDropped += RewardCollectComponent_OnRewardDropped;

        }


        public void Deinitialize()
        {
            CoinCollectComponent.OnShouldCollectCoin -= CoinCollectComponent_OnShouldCollectCoin;
            RewardCollectComponent.OnRewardDropped -= RewardCollectComponent_OnRewardDropped;
            DestroyAnnouncers();
        }


        public void ReturnToInitialState()
        {
            DestroyAnnouncers();
        }


        private void DestroyAnnouncers()
        {
            DOTween.Kill(this);

            announcers.RemoveAll(i => i == null);

            if (announcers.Count > 0)
            {
                ComponentPool pool = PoolManager.Instance.GetComponentPool(announcers.First());

                announcers.ForEach(i =>
                {
                    if (pool != null)
                    {
                        pool.Push(i);
                    }
                });
            }

            announcers.Clear();
        }


        private void PlayAnnouncer(Vector3 startPosition, float currencyCount)
        {
            settings.announcerMoveAnimation.SetupBeginValue(startPosition);
            settings.announcerMoveAnimation.SetupEndValue(startPosition + settings.announcerOffsetMove);

            TMP_Text announcer = Content.Management.CreateCurrencyAnnouncer(startPosition);
            string announcerText = string.Format("+{0}", currencyCount);
            announcer.SetText(announcerText);

            settings.announcerAlphaAnimation.Play((value) => announcer.color = announcer.color.SetA(value), this);
            settings.announcerScaleAnimation.Play((value) => announcer.transform.localScale = value, this);
            settings.announcerMoveAnimation.Play((value) => announcer.transform.position = value, this);

            announcers.Add(announcer);
        }

        #endregion



        #region Events handlers

        private void CoinCollectComponent_OnShouldCollectCoin(CurrencyLevelObject coin, ICoinCollector collector) =>
            PlayAnnouncer(coin.transform.position, coin.CurrencyCount);


        private void RewardCollectComponent_OnRewardDropped(PhysicalLevelObject physicalLevel, BonusLevelObjectData bonusLevelObjectData)
        {
            if (bonusLevelObjectData.rewardType != Proposal.RewardType.Currency)
            {
                return;
            }

            float count = bonusLevelObjectData.currencyAmount;

            PlayAnnouncer(physicalLevel.transform.position, count);
        }
        
        #endregion
    }
}
