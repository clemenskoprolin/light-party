using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace LightParty.Services
{
    /// <summary>
    /// This class contrains the methods which send and receive data from servers.
    /// </summary>
    class InternetRequests
    {
        /// <summary>
        /// Trys to send a post request to a given URL with a json string.
        /// </summary>
        /// <param name="url">The URL to which the request should be sent to</param>
        /// <param name="json">The string in json format</param>
        /// <returns>The reseviced data from the server. If a error occurs, 'null' will be returned.</returns>
        public static async Task<string> SendJsonPostRequest(string url, string json)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri(url);

                HttpStringContent content = new HttpStringContent(json);

                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(uri, content);
                httpResponseMessage.EnsureSuccessStatusCode();
                string httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();

                return httpResponseBody;
            }
            catch
            {
                return "null";
            }
        }

        /// <summary>
        /// Trys to send a get request to a given URL.
        /// </summary>
        /// <param name="url">The URL to which the request should be sent to</param>
        /// <returns>The reseviced data from the server. If a error occurs, 'null' will be returned.</returns>
        public static async Task<string> SendGetRequest(string url)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri(url);

                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

                httpResponseMessage = await httpClient.GetAsync(uri);
                httpResponseMessage.EnsureSuccessStatusCode();
                return await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch
            {
                return "null";
            }
        }
    }
}
