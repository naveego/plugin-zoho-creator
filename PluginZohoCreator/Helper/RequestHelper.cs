using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PluginZohoCreator.Helper
{
    public class RequestHelper
    {
        private readonly Authenticator _authenticator;
        private readonly HttpClient _client;
        
        public RequestHelper(Settings settings, HttpClient client)
        {
            _authenticator = new Authenticator(settings);
            _client = client;
        }

        /// <summary>
        /// Get Async request wrapper for making authenticated requests
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            string token;

            // get the token
            try
            {
                token = _authenticator.GetToken();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
            
            // add token to the request and execute the request
            try
            {
                var uriObj = new Uri(uri);

                if (!uriObj.Query.Contains("authtoken"))
                {
                    if (!String.IsNullOrEmpty(uriObj.Query))
                    {
                        uri = String.Format("{0}&authtoken={1}", uri, token);
                    }
                    else
                    {
                        uri = String.Format("{0}?authtoken={1}", uri, token);
                    }
                }
                var client = _client;

                var response = await client.GetAsync(uri);

                return response;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }
        
        public async Task<HttpResponseMessage> PostAsync(string uri, StringContent json)
        {
            string token;

            // get the token
            try
            {
                token = _authenticator.GetToken();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
            
            // add token to the request and execute the request
            try
            {
                var uriObj = new Uri(uri);

                if (!uriObj.Query.Contains("authtoken"))
                {
                    if (!String.IsNullOrEmpty(uriObj.Query))
                    {
                        uri = String.Format("{0}&authtoken={1}", uri, token);
                    }
                    else
                    {
                        uri = String.Format("{0}?authtoken={1}", uri, token);
                    }
                }
                
                var client = _client;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsync(uri, json);

                return response;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutAsync(string uri, StringContent json)
        {
            string token;

            // get the token
            try
            {
                token = _authenticator.GetToken();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
            
            // add token to the request and execute the request
            try
            {
                var uriObj = new Uri(uri);

                if (!uriObj.Query.Contains("authtoken"))
                {
                    if (!String.IsNullOrEmpty(uriObj.Query))
                    {
                        uri = String.Format("{0}&authtoken={1}", uri, token);
                    }
                    else
                    {
                        uri = String.Format("{0}?authtoken={1}", uri, token);
                    }
                }
                
                var client = _client;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PutAsync(uri, json);

                return response;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }
    }
}