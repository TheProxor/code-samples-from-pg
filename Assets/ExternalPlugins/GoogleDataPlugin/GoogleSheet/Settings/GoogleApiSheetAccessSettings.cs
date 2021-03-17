using UnityEngine;
using ButtonAttribute = Google.ApiPlugin.GoogleApiSheetAccessButtonAttribute;


namespace Google.ApiPlugin.Sheets
{
    public class GoogleApiSheetAccessSettings : ScriptableObject
    {

        #region Fields

        [Header("Common Fields")]
        [SerializeField] private string clientID = default;
        [SerializeField] private string clientSecret = default;

        [SerializeField] private string refreshToken = default;
        [SerializeField] private string accessToken = default;

        [Header("Custom Fields")]
        [SerializeField] private string googleSheetId = default;
        [SerializeField] private int googleSheetGId = default;

        // just for debug
        [TextArea]
        [SerializeField] private string googleSheetOutput = default;

        #endregion



        #region Methods

        [Button("Refresh Token")]
        public async void RefreshToken()
        {
            string token = await GoogleApiSheetAccess.RefreshToken(refreshToken, clientID, clientSecret);
            accessToken = token;
        }


        [Button("Access Google Sheet")]
        public async void AccessGoogleSheet()
        {
            string sheetData = await GoogleApiSheetAccess.AccessGoogleSheet(accessToken, googleSheetId, googleSheetGId);

            googleSheetOutput = sheetData;
        }

        #endregion
    }
}
