using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginZohoCreator.DataContracts
{
    public class CustomFieldsResponse
    {
        [JsonProperty("response")]
        public Dictionary<string, object> Response { get; set; }
    }
    
    public class CustomFieldsObject
    {
        [JsonProperty("fields")]
        public List<CustomField> Fields { get; set; }
    }
    
    public class CustomField
    {
        [JsonProperty("type")]
        public int Type { get; set; }
        
        [JsonProperty("maxchar")]
        public int MaxChar { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }
        
        [JsonProperty("unique")]
        public bool Unique { get; set; }
        
        [JsonProperty("fieldname")]
        public string FieldName { get; set; }
        
        [JsonProperty("displayname")]
        public string DisplayName { get; set; }
    }
}