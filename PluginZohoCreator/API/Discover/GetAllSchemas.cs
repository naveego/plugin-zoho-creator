using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginZohoCreator.DataContracts;
using PluginZohoCreator.Helper;
using Pub;

namespace PluginZohoCreator.API.Discover
{
    public static partial class Discover
    {
        /// <summary>
        /// Gets all schemas
        /// </summary>
        /// <param name="client"></param>
        /// <returns>A list of schemas</returns>
        public static async Task<List<Schema>> GetAllSchemas(RequestHelper client)
        {
            ApplicationsResponse applicationsResponse;

            // get the applications present in Zoho
            try
            {
                Logger.Debug("Getting applications...");

                var response =
                    await client.GetAsync("https://creator.zoho.com/api/json/applications?scope=creatorapi");
                response.EnsureSuccessStatusCode();

                applicationsResponse =
                    JsonConvert.DeserializeObject<ApplicationsResponse>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
            // attempt to get a schema for each view in each application
            try
            {
                Logger.Info(
                    $"Applications attempted: {applicationsResponse.Result.ApplicationsList.ApplicationsObjects.First().Applications.Count}");

                var tasks = applicationsResponse.Result.ApplicationsList.ApplicationsObjects.First().Applications
                    .Select(x =>
                        GetSchemasForApplication(client, x, applicationsResponse.Result.ApplicationOwner))
                    .ToArray();

                await Task.WhenAll(tasks);

                return tasks.SelectMany(x => x.Result).ToList();
                
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }
    }
}