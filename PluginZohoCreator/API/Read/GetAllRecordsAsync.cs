using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Naveego.Sdk.Logging;
using Newtonsoft.Json;
using PluginZohoCreator.DataContracts;
using PluginZohoCreator.Helper;


namespace PluginZohoCreator.API.Read
{
    public static partial class Read
    {
        public static async Task<List<Dictionary<string, object>>> GetAllRecordsAsync(RequestHelper client,
            PublisherMetaJson publisherMetaJson)
        {
            //https://www.zoho.com/creator/help/api/rest-api/rest-api-view-records-in-view.html
            try
            {
                var pageSize = 200;
                var index = 0;
                var readMore = true;
                var recordsOut = new List<Dictionary<string, object>>();

                while (readMore)
                {
                    // get records for schema page by page
                    var uri = String.Format(
                        "https://creator.zoho.com/api/json/{0}/view/{1}?scope=creatorapi&zc_ownername={2}&startindex={3}&limit={4}&raw=true",
                        publisherMetaJson.ApplicationName,
                        publisherMetaJson.ViewName,
                        publisherMetaJson.OwnerName,
                        index,
                        pageSize
                    );

                    var response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();

                    // if response is empty or call did not succeed return no records
                    if (!await Utility.Utility.IsSuccessAndNotEmpty(response))
                    {
                        return recordsOut;
                    }

                    var recordsResponse =
                        JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(
                            await response.Content.ReadAsStringAsync());

                    //Logger.Debug($"data: {JsonConvert.SerializeObject(recordsResponse[publisherMetaJson.FormName])}"); 

                    recordsOut.AddRange(recordsResponse[publisherMetaJson.FormName]);

                    if (recordsResponse[publisherMetaJson.FormName].Count == 0)
                    {
                        readMore = false;
                    }

                    index += pageSize;
                }
                
                return recordsOut;
            }
            catch (Exception e)
            {
                Logger.Error(e, "GetAllRecordsAsync");
                Logger.Error(e, e.Message);
                throw;
            }
        }
    }
}