using UnityEngine;
using Google.ApiPlugin.Sheets;
using System;


namespace Drawmasters
{
    public class CommonContentSettings : ScriptableObject
    {
        #region Fields

        [Header("Google Data Fields")]
        [SerializeField] private string clientID = default;
        [SerializeField] private string clientSecret = default;

        [SerializeField] private string refreshToken = default;

        #endregion



        #region Properties

        public string AccessToken { get; private set; }

        #endregion



        #region Methods

        public async void RefreshGoogleToken(Action onCompleted = default)
        {
            AccessToken = await GoogleApiSheetAccess.RefreshToken(refreshToken, clientID, clientSecret);
            onCompleted?.Invoke();
        }
        #endregion

    }
}
