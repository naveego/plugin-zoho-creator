using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PluginZohoCreator.Helper;
using Pub;

namespace PluginZohoCreator.API.Discover
{
    public static partial class Discover
    {
        /// <summary>
        /// Gets all custom schemas specified by the config
        /// </summary>
        /// <param name="client"></param>
        /// <param name="customUrlList"></param>
        /// <returns>A list of schemas</returns>
        public static async Task<List<Schema>> GetAllCustomSchemas(RequestHelper client, List<CustomUrlObject> customUrlList)
        {
            try
            {
                Logger.Info(
                    $"Custom schemas attempted: {customUrlList.Count}");

                var tasks = customUrlList
                    .Select(x => GetCustomSchema(client, x))
                    .ToArray();

                await Task.WhenAll(tasks);

                return tasks.Where(x => x.Result != null).Select(x => x.Result).ToList();
            }
            catch (Exception e)
            {
                Logger.Error($"Get All Custom Schemas: {e.Message}");
                throw;
            }
        }
    }
}