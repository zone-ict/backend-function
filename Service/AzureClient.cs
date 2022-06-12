
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;

namespace Com.ZoneIct
{
    public class AzureClient
    {
        static readonly string key = Environment.GetEnvironmentVariable("COGNITIVE_TRANSLATION_KEY");
        static readonly string location = "japaneast";
        static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";
        static HttpClient _client = new HttpClient();
        public async static Task<string> Translate(string textToTranslate, string toLang)
        {
            string surfix = "\n\n🌐 Translate";
            string route = $"/translate?api-version=3.0&to={toLang}";
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);
            string translated;

            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                var response = await Policy.Handle<HttpRequestException>()
                    .Or<SocketException>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 1))
                    .ExecuteAsync(() => _client.SendAsync(request.Clone()));

                var ret = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(ret);
                translated = obj[0].translations[0].text;

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new HttpRequestException($"{response.StatusCode.ToString()} {ret}");
            }
            return translated + surfix;
        }
    }
}