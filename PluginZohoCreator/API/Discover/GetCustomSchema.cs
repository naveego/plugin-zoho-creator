using System;
using System.Collections.Generic;
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
        /// Gets a custom schema based on provided config information
        /// </summary>
        /// <param name="client"></param>
        /// <param name="customUrlObject"></param>
        /// <returns>A custom schema</returns>
        private static async Task<Schema> GetCustomSchema(RequestHelper client, CustomUrlObject customUrlObject)
        {
            var schemaName = $"Custom {customUrlObject.ApplicationName} {customUrlObject.ViewName}";
            
            // base schema to be added to
            var schema = new Schema
            {
                Id = $"custom-{customUrlObject.ApplicationName}-{customUrlObject.ViewName}",
                Name = schemaName,
                Description = "",
                PublisherMetaJson = JsonConvert.SerializeObject(new PublisherMetaJson
                {
                    ApplicationName = customUrlObject.ApplicationName,
                    OwnerName = customUrlObject.ApplicationOwner,
                    ViewName = customUrlObject.ViewName,
                    FormName = customUrlObject.FormName
                }),
                DataFlowDirection = Schema.Types.DataFlowDirection.Read
            };

            try
            {
                Logger.Debug($"Getting fields for: {schemaName}");

                // get fields for module
                var uri = String.Format(
                    "https://creator.zoho.com/api/{0}/json/{1}/form/{2}/fields?scope=creatorapi",
                    customUrlObject.ApplicationOwner,
                    customUrlObject.ApplicationName,
                    customUrlObject.FormName
                );
                var response = await client.GetAsync(uri);

                // if response is empty or call did not succeed return null
                if (! await Utility.Utility.IsSuccessAndNotEmpty(response))
                {
                    Logger.Info($"No fields for: {schemaName}");
                    return null;
                }

                Logger.Debug($"Got fields for: {schemaName}");

                // for each field in the schema add a new property
                var fieldsResponse =
                    JsonConvert.DeserializeObject<CustomFieldsResponse>(await response.Content.ReadAsStringAsync());

                var fieldsObject =
                    JsonConvert.DeserializeObject<CustomFieldsObject>(
                        JsonConvert.SerializeObject(fieldsResponse.Response));

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
                        TypeAtSource = field.Type.ToString(),
                        IsNullable = !field.Required
                    };

                    schema.Properties.Add(property);
                }

                Logger.Debug($"Added schema for: {schemaName}");
                return schema;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Get Custom Schema: {e.Message}");
                return null;
            }
        }
    }
}