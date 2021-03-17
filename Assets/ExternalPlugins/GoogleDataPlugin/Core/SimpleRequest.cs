using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Networking;


namespace Google.ApiPlugin
{
    public class SimpleRequest
    {
        private Dictionary<string, string> headers;

        public void SetupHeaders(Dictionary<string, string> _headers = default) =>
            headers = _headers;


        public async Task SendRequest(string url, string webMethod = UnityWebRequest.kHttpVerbGET,
                                                  Action<UnityWebRequest> onCompleted = null)
        {
            var webRequest = new UnityWebRequest(url)
            {
                method = webMethod,
                timeout = 5,
                downloadHandler = new DownloadHandlerBuffer()
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }

            var requestAsyncOperation = webRequest.SendWebRequest();

            while (!requestAsyncOperation.isDone)
            {
                await Task.Delay(100);
            }

            onCompleted?.Invoke(webRequest);
        }
    }
}