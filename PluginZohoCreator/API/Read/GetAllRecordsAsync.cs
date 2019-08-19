using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginZohoCreator.DataContracts;
using PluginZohoCreator.Helper;
using Pub;

namespace PluginZohoCreator.API.Read
{
    public static partial class Read
    {
        public static async Task<Dictionary<string, List<Dictionary<string, object>>>> GetAllRecordsAsync(RequestHelper client, PublisherMetaJson publisherMetaJson)
        {
            try
            {
                Dictionary<string, List<Dictionary<string, object>>> recordsResponse = new Dictionary<string, List<Dictionary<string, object>>>();

                // get records for schema page by page
                var uri = String.Format(
                    "https://creator.zoho.com/api/json/{0}/view/{1}?scope=creatorapi&zc_ownername={2}&raw=true",
                    publisherMetaJson.ApplicationName,
                    publisherMetaJson.ViewName,
                    publisherMetaJson.OwnerName
                );

                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                // if response is empty or call did not succeed return no records
                if (! await Utility.Utility.IsSuccessAndNotEmpty(response))
                {
                    return recordsResponse;
                }

                recordsResponse =
                    JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(
                        await response.Content.ReadAsStringAsync());

                Logger.Debug($"data: {JsonConvert.SerializeObject(recordsResponse[publisherMetaJson.FormName])}");

                return recordsResponse;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }
    }
}