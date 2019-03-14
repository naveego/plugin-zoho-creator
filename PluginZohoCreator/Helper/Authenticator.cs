using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PluginZohoCreator.Helper
{
    public class Authenticator
    {
        private readonly string _token;

        public Authenticator(Settings settings)
        {
            _token = settings.Token;
        }

        /// <summary>
        /// Get a token for the Zoho Creator API
        /// </summary>
        /// <returns></returns>
        public string GetToken()
        {
            // return saved token
            return _token;
        }
    }
}