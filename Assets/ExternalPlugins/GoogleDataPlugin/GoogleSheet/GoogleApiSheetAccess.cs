using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;


namespace Google.ApiPlugin.Sheets
{
    public static class GoogleApiSheetAccess
    {
        public static bool IsLogsEnabled = false;

        private const string RefreshTokenUrlFormat = "{0}?grant_type=refresh_token&refresh_token={1}&client_id={2}&client_secret={3}";
        private const string AccessTokenUrl = "https://accounts.google.com/o/oauth2/token";

        private const string UrlGoogleCsvExportFormat = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";



        public static async Task<string> RefreshToken(string refreshToken, string clientID, string clientSecret)
        {
            string result = string.Empty;

            string url = string.Format(RefreshTokenUrlFormat, AccessTokenUrl, refreshToken, clientID, clientSecret);

            SimpleRequest request = new SimpleRequest();
            await request.SendRequest(url, UnityWebRequest.kHttpVerbPOST, (web) =>
            {
                AttemptLog($"RefreshToken response code:{web.responseCode}");

                if (web.responseCode == (long)HttpStatusCode.OK)
                {
                    JObject json = JObject.Parse(web.downloadHandler.text);
                    string receivedAccessToken = json.GetValue("access_token").ToString();

                    AttemptLog($"<color=green>Access token received successfully:</color>\n{receivedAccessToken}");

                    result = receivedAccessToken;
                }
            });

            return result;
        }


        public static async Task<string> AccessGoogleSheet(string accessToken, string googleSheedDocId, int gid = 0)
        {
            string result = string.Empty;
            string url = string.Format(UrlGoogleCsvExportFormat, googleSheedDocId, gid);

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" }
            };

            SimpleRequest request = new SimpleRequest();
            request.SetupHeaders(headers);

            await request.SendRequest(url, UnityWebRequest.kHttpVerbGET, (web) =>
            {
                AttemptLog($"Access response code:{web.responseCode}");

                if (web.responseCode == (long)HttpStatusCode.OK)
                {
                    AttemptLog($"<color=green>Accessed spreadsheet received successfully:</color>\n" +
                              $"googleSheedDocId={googleSheedDocId}\n" +
                              $"gid={gid}");
                    result = web.downloadHandler.text;
                }
            });

            return result;
        }


        private static void AttemptLog(string log)
        {
            if (IsLogsEnabled)
            {
                Debug.Log(log);
            }
        }
    }
}
