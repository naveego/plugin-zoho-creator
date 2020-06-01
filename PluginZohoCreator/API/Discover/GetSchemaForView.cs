using System;
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
        /// Gets a schema for a given form
        /// </summary>
        /// <param name="client"></param>
        /// <param name="view"></param>
        /// <param name="applicationName"></param>
        /// <param name="applicationOwner"></param>
        /// <returns>returns a schema or null if unavailable</returns>
        private static async Task<Schema> GetSchemaForView(RequestHelper client, View view, string applicationName, string applicationOwner)
        {
            // base schema to be added to
            var schema = new Schema
            {
                Id = $"{applicationName}-{view.ComponentName}",
                Name = $"{applicationName} {view.DisplayName}",
                Description = "",
                PublisherMetaJson = JsonConvert.SerializeObject(new PublisherMetaJson
                {
                    ApplicationName = applicationName,
                    OwnerName = applicationOwner,
                    ViewName = view.ComponentName,
                    FormName = view.FormLinkName
                }),
                DataFlowDirection = Schema.Types.DataFlowDirection.Read
            };

            try
            {
                Logger.Debug($"Getting fields for: {view.DisplayName}");

                // get fields for module
                var uri = String.Format(
                    "https://creator.zoho.com/api/json/{0}/{1}/fields?scope=creatorapi&zc_ownername={2}",
                    applicationName,
                    view.FormLinkName,
                    applicationOwner
                );
                var response = await client.GetAsync(uri);

                // if response is empty or call did not succeed return null
                if (! await Utility.Utility.IsSuccessAndNotEmpty(response))
                {
                    Logger.Debug($"No fields for: {view.DisplayName}\n{await response.Content.ReadAsStringAsync()}");
                    return null;
                }

                Logger.Debug($"Got fields for: {view.DisplayName}");

                // for each field in the schema add a new property
                var fieldsResponse =
                    JsonConvert.DeserializeObject<FieldsResponse>(await response.Content.ReadAsStringAsync());

                var formNameObject =
                    JsonConvert.DeserializeObject<FormNameObject>(
                        JsonConvert.SerializeObject(fieldsResponse.ApplicationName[1]));

                var fieldsObject =
                    JsonConvert.DeserializeObject<FormName>(JsonConvert.SerializeObject(formNameObject.FormName[1]));

                var key = new Property
                {
                    Id = "ID",
                    Name = "ID",
                    Type = PropertyType.String,
                    IsKey = true,
                    IsCreateCounter = false,
                    IsUpdateCounter = false,
                    TypeAtSource = "ID",
                    IsNullable = false
                };

                schema.Properties.Add(key);
                
                foreach (var field in fieldsObject.Fields)
                {
                    var property = new Property
                    {
                        Id = field.FieldName,
                        Name = field.DisplayName,
                        Type = GetPropertyType(field),
                        IsKey = field.Unique,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = field.ApiType.ToString(),
                        IsNullable = !field.Unique
                    };

                    schema.Properties.Add(property);
                }

                Logger.Debug($"Added schema for: {applicationName} {view.DisplayName}");
                return schema;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Get Schema For View: {e.Message}");
                return null;
            }
        }
    }
}