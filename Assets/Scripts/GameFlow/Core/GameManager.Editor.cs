using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;
using Drawmasters.Ui;
using UnityEngine;


namespace Drawmasters
{
    public partial class GameManager
    {
        #region Editor methods

        #if UNITY_EDITOR
        private void Update()
        {
            
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Time.timeScale = 0.1f;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                Time.timeScale = 0.2f;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Time.timeScale = 0.5f;
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1.0f;
            }

            var v = GameServices.Instance.LevelEnvironment;
            if (v == null)
            {
                return;
            }
            var c = v.Context;
            if (c == null || c.IsEditor)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                ServiceUtil.GameServices.Instance.ProposalService.HitmastersProposeController.Propose();
            }
        }

        #endif

        [Sirenix.OdinInspector.Button]
        public void SetSkulls(float count)
        {
            float current = GameServices.Instance.PlayerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.Skulls);
            GameServices.Instance.PlayerStatisticService.CurrencyData.TryRemoveCurrency(CurrencyType.Skulls, current);
            GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(CurrencyType.Skulls, count);
        }
        #endregion
    }
}
