using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PluginZohoCreator.API.Utility
{
    public static partial class Utility
    {
        /// <summary>
        /// Checks if a http response message is not empty and did not fail
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<bool> IsSuccessAndNotEmpty(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (content.Contains("<!DOCTYPE HTML>"))
            {
                return false;
            }
            return response.StatusCode != HttpStatusCode.NoContent && response.IsSuccessStatusCode;
        }
    }
}