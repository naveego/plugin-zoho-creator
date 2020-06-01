using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginZohoCreator.DataContracts;
using PluginZohoCreator.Helper;


namespace PluginZohoCreator.API.Discover
{
    public static partial class Discover
    {
        /// <summary>
        /// Gets all schemas for a given application
        /// </summary>
        /// <param name="client"></param>
        /// <param name="application"></param>
        /// <param name="applicationOwner"></param>
        /// <returns></returns>
        private static async Task<List<Schema>> GetSchemasForApplication(RequestHelper client, Application application, string applicationOwner)
        {
            FormsAndViewsResponse formsAndViewsResponse;

            // get the forms and views present in application
            try
            {
                Logger.Debug($"Getting forms and views for {application.ApplicationName}...");

                var uri = String.Format(
                    "https://creator.zoho.com/api/json/{0}/formsandviews?scope=creatorapi&zc_ownername={1}",
                    application.LinkName,
                    applicationOwner
                );
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                formsAndViewsResponse =
                    JsonConvert.DeserializeObject<FormsAndViewsResponse>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Logger.Error(e, "Get All Schemas for Application 1");
                Logger.Error(e, e.Message);
                throw;
            }

            // get all schemas for each application
            try
            {
                var formsAndViewsObject =
                    JsonConvert.DeserializeObject<FormsAndViewsObject>(
                        JsonConvert.SerializeObject(formsAndViewsResponse.ApplicationName[1]));

                var tasks = formsAndViewsObject.ViewList.Where(f => f.FormLinkName != null)
                    .Select(x => GetSchemaForView(client, x, application.LinkName, applicationOwner))
                    .ToArray();

                await Task.WhenAll(tasks);

                return tasks.Where(x => x.Result != null).Select(x => x.Result).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Get All Schemas for Application 2");
                Logger.Error(e, e.Message);
                throw;
            }
        }
    }
}