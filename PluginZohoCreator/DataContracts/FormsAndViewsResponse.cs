using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginZohoCreator.DataContracts
{
    public class FormsAndViewsResponse
    {
        [JsonProperty("application-name")]
        public List<FormsAndViewsObject> ApplicationName { get; set; }   
    }

    public class FormsAndViewsObject
    {
        [JsonProperty("viewList")]
        public List<View> ViewList { get; set; }
        
        [JsonProperty("formList")]
        public List<Form> FormList { get; set; }
    }

    public class View
    {
        [JsonProperty("linkid")]
        public int LinkId { get; set; }
        
        [JsonProperty("displayname")]
        public string DisplayName { get; set; }
        
        [JsonProperty("componentname")]
        public string ComponentName { get; set; }
        
        [JsonProperty("formlinkname")]
        public string FormLinkName { get; set; }
    }

    public class Form
    {
        [JsonProperty("linkid")]
        public int LinkId { get; set; }
        
        [JsonProperty("displayname")]
        public string DisplayName { get; set; }
        
        [JsonProperty("componentname")]
        public string ComponentName { get; set; }
    }
}