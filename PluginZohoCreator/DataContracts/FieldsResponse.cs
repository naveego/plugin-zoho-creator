using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginZohoCreator.DataContracts
{
    public class FieldsResponse
    {
        [JsonProperty("application-name")]
        public List<FormNameObject> ApplicationName { get; set; }
    }

    public class FormNameObject
    {
        [JsonProperty("form-name")]
        public List<FormName> FormName { get; set; }
    }
    
    public class FormName
    {
        [JsonProperty("Fields")]
        public List<Field> Fields { get; set; }
        
        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }
    }

    public class Field
    {
        [JsonProperty("Reqd")]
        public bool Reqd { get; set; }
        
        [JsonProperty("Type")]
        public int Type { get; set; }
        
        [JsonProperty("Tooltip")]
        public string ToolTip { get; set; }
        
        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }
        
        [JsonProperty("Unique")]
        public bool Unique { get; set; }
        
        [JsonProperty("MaxChar")]
        public int MaxChar { get; set; }
        
        [JsonProperty("FieldName")]
        public string FieldName { get; set; }
        
        [JsonProperty("Initial")]
        public string Initial { get; set; }
        
        [JsonProperty("CurrencyType")]
        public string CurrencyType { get; set; }
        
        [JsonProperty("apiType")]
        public int ApiType { get; set; }
    }
}