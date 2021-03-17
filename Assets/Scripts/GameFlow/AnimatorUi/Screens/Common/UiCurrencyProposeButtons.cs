using UnityEngine;
using UnityEngine.UI;
using System;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Ui
{
    public class UiCurrencyProposeButtons : MonoBehaviour
    {
        #region Nested types

        [Serializable]
        private class Data
        {
            public CurrencyType currencyType = default;
            public Button proposeButton = default;
        }

        #endregion



        #region Fields

        [SerializeField] private Data[] data = default;

        #endregion



        #region Public methods

        public void Initialize()
        {
            foreach (var d in data)
            {
                // TODO: refactor. think about using event system
                d.proposeButton.onClick.AddListener(() => ProposeButton_onClick(d.currencyType));
            }
        }


        public void Deinitialize()
        {
            foreach (var d in data)
            {
                d.proposeButton.onClick.RemoveAllListeners();
            }
        }

        #endregion



        #region Events handlers

        private void ProposeButton_onClick(CurrencyType currencyType) =>
            GameServices.Instance.ProposalService.CurrencyShopProposal.Propose(currencyType);
        
        #endregion
    }
}