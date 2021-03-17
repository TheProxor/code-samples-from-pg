using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Google.ApiPlugin.Sheets;
using Object = UnityEngine.Object;


namespace Drawmasters.Utils
{
    public static class CSVDownloader
    {
        private const string UrlGoogleCsvExportFormat = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";

        /// <summary>
        /// Download google sheet as csv by id in to local path (under Assets)
        /// </summary>
        public static async Task DownloadDataAsync(string googleSheedDocId, string saveDataPath, int gid = 0, Action onCompleted = null)
        {
            string url = string.Format(UrlGoogleCsvExportFormat, googleSheedDocId, gid);
            string saveFilePath = string.Concat(Application.dataPath, saveDataPath);

            //TODO: maybe uncomment if access denied
            //  File.SetAttributes(resultFile, FileAttributes.Normal);
            DownloadHandlerFile downloadHandler = new DownloadHandlerFile(saveFilePath);

            var webRequest = new UnityWebRequest(url)
            {
                method = UnityWebRequest.kHttpVerbGET,
                downloadHandler = downloadHandler
            };

            var requestAsyncOperation = webRequest.SendWebRequest();

            while (!requestAsyncOperation.isDone)
            {
                await Task.Delay(100);
            }

            onCompleted?.Invoke();
        }


        /// <summary>
        /// Read google sheet as csv by id and gid
        /// </summary>
        public static async Task ReadDataAsync(string googleSheedDocId, int gid = 0, Action<string> onCompleted = null)
        {
            var contentSettings = ResourcesUtility.LoadAssetsByType(typeof(CommonContentSettings)).FirstObject() as CommonContentSettings;
            string result = await GoogleApiSheetAccess.AccessGoogleSheet(contentSettings.AccessToken, googleSheedDocId, gid);
            onCompleted?.Invoke(result);
        }
    }
}
