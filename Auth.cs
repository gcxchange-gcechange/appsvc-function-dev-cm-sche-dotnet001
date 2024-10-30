using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace appsvc_function_dev_cm_sche_dotnet001
{
    public class ROPCConfidentialTokenCredential : TokenCredential
    {
        string _username = "";
        string _password = "";
        string _tenantId = "";
        string _clientId = "";
        string _clientSecret = "";
        string _tokenEndpoint = "";
        ILogger _log;

        public ROPCConfidentialTokenCredential(ILogger log)
        {
            _log = log;

            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };

            var client = new SecretClient(new System.Uri(Globals.keyVaultUrl), new DefaultAzureCredential(), options);
            KeyVaultSecret secret_client = client.GetSecret(Globals.secretNameClient);
            var clientSecret = secret_client.Value;
            KeyVaultSecret secret_password = client.GetSecret(Globals.secretNameDelegatePassword);
            var password = secret_password.Value;

            // Public Constructor
            _username = Globals.delegateEmail;
            _password = password;
            _tenantId = Globals.tenantId;
            _clientId = Globals.clientId;
            _clientSecret = clientSecret;

            _tokenEndpoint = "https://login.microsoftonline.com/" + _tenantId + "/oauth2/v2.0/token";
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            HttpClient httpClient = new HttpClient();

            // Create the request body
            var Parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("scope", string.Join(" ", requestContext.Scopes)),
                new KeyValuePair<string, string>("username", _username),
                new KeyValuePair<string, string>("password", _password),
                new KeyValuePair<string, string>("grant_type", "password")
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(Parameters)
            };
            var response = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
            dynamic responseJson = JsonConvert.DeserializeObject(response);
            var expirationDate = DateTimeOffset.UtcNow.AddMinutes(60.0);
            return new AccessToken(responseJson.access_token.ToString(), expirationDate);
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            HttpClient httpClient = new HttpClient();

            // Create the request body
            var Parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("scope", string.Join(" ", requestContext.Scopes)),
                new KeyValuePair<string, string>("username", _username),
                new KeyValuePair<string, string>("password", _password),
                new KeyValuePair<string, string>("grant_type", "password")
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(Parameters)
            };
            var response = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
            dynamic responseJson = JsonConvert.DeserializeObject(response);

            var expirationDate = DateTimeOffset.UtcNow.AddMinutes(60.0);
            return new ValueTask<AccessToken>(new AccessToken(responseJson.access_token.ToString(), expirationDate));
        }
    }
}
