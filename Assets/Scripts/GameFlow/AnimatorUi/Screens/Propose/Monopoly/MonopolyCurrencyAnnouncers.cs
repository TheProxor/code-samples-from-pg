using System.Collections.Generic;
using DG.Tweening;
using Drawmasters.Pool;
using Drawmasters.Proposal;
using Drawmasters.Announcer;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class MonopolyCurrencyAnnouncers : IDeinitializable
    {
        #region Fields

        private CurrencyAnnouncerSettings settings;

        private readonly List<MonopolyCurrencyAnnouncer> announcers = new List<MonopolyCurrencyAnnouncer>();

        #endregion
        
        
        
        #region Public methods

        public void Initialize()
        {
            settings = IngameData.Settings.hitmasters.currencyAnnouncerSettings;
        }
        
        
        public void PlayAnnouncer(Transform root, CurrencyReward currencyRewardData) =>
            PlayAnnouncer(root, currencyRewardData.currencyType, currencyRewardData.value);
        
        #endregion
        
        
        
        #region IDeinitialize

        public void Deinitialize()
        {
            announcers.RemoveAll(i => i.IsNull());

            if (announcers.Count > 0)
            {
                ComponentPool pool = PoolManager.Instance.GetComponentPool(announcers.First());

                announcers.ForEach(i =>
                {
                    i.Deinitialize();
                    
                    pool?.Push(i);
                });
            }

            announcers.Clear();
        }
        
        #endregion
        
        
        
        #region Private methods

        private void PlayAnnouncer(Transform root, CurrencyType currencyType, float currencyCount)
        {
            MonopolyCurrencyAnnouncer announcer = Content.Management.CreateCurrencyMonopolyAnnouncer(root);

            Sprite announcerIconSprite = settings.FindSprite(currencyType);

            string targetText = $"+{currencyCount}";

            announcer.SetupValues(announcerIconSprite, targetText);
            
            announcer.SetupData(settings.announcerMoveAnimation,
                                settings.announcerAlphaAnimation,
                                settings.announcerScaleAnimation);
            
            announcer.PlayLocal(Vector3.zero, settings.announcerOffsetMove);

            announcers.Add(announcer);
        }
        
        #endregion
    }
}
