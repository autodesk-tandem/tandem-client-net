using Newtonsoft.Json;

namespace Autodesk.Tandem.Client.Utils
{
    public class AuthToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public abstract class AuthBase
    {
        public AuthBase(string clientID, string clientSecret, string scope)
        {
            ClientID = clientID;
            ClientSecret = clientSecret;
            Scope = scope;
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://developer.api.autodesk.com")
            };
        }

        protected string ClientID { get; private set; }
        protected string ClientSecret { get; private set; }
        protected string Scope { get; private set; }

        protected HttpClient HttpClient { get; private set; }

        abstract protected FormUrlEncodedContent GetContent();

        public async Task<T?> GetTokenAsync<T>()
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "authentication/v2/token");
            var auth = $"{ClientID}:{ClientSecret}";

            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth)));
            req.Content = GetContent();
            var response = await HttpClient.SendAsync(req);
            var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            var result = serializer.Deserialize<T>(jsonReader);

            return result;
        }
    }
}
