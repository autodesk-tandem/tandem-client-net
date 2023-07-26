namespace Autodesk.Tandem.Client.Utils
{
    public class TwoLeggedAuth : AuthBase
    {
        private string accessToken;
        private DateTime expirationTime;

        public TwoLeggedAuth(string clientID, string clientSecret, string scope)
            : base(clientID, clientSecret, scope)
        {
        }

        public string GetToken()
        {
            if (string.IsNullOrEmpty(accessToken) || ((expirationTime - DateTime.Now).TotalSeconds < 10))
            {
                var result = GetTokenAsync<AuthToken>().ConfigureAwait(false).GetAwaiter().GetResult();

                if (result == null)
                {
                    return string.Empty;
                }
                expirationTime = DateTime.Now.AddSeconds(result.ExpiresIn);
                accessToken = result.AccessToken;
            }
            return accessToken;
        }

        protected override FormUrlEncodedContent GetContent()
        {
            var fields = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", Scope)
        };
            return new FormUrlEncodedContent(fields);
        }
    }
}
