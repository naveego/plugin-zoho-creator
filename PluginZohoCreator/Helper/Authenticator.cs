using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PluginZohoCreator.Helper
{
    public class Authenticator
    {
        private readonly HttpClient _client;
        private readonly Settings _settings;
        private string              _token;

        public Authenticator(Settings settings, HttpClient client)
        {
            _client = client;
            _settings = settings;
            _token = String.Empty;
        }

        /// <summary>
        /// Get a token for the Zoho API
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetToken()
        {
            // check if token empty
            if (String.IsNullOrEmpty(_token))
            {
                try
                {
                    // get a token
                    var requestUri = String.Format(
                        "https://accounts.zoho.com/apiauthtoken/nb/create?SCOPE=ZohoCreator/creatorapi&EMAIL_ID={0}&PASSWORD={1}",
                        _settings.Username,
                        _settings.Password);
            
                    var response = await _client.PostAsync(requestUri, null);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    
                    // update saved token
                    _token = content.Split("\n").First(t => t.Contains("AUTHTOKEN")).Split("=")[1];

                    return _token;
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    throw;
                }
            }
            // return saved token
            return _token;
        }
    }
}