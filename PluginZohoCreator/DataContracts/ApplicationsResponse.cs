using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginZohoCreator.DataContracts
{
    public class ApplicationsResponse
    {
        [JsonProperty("result")]
        public ApplicationsResult Result { get; set; }
    }

    public class ApplicationsResult
    {
        [JsonProperty("application_owner")]
        public string ApplicationOwner { get; set; }
        
        [JsonProperty("applications_list")]
        public ApplicationsList ApplicationsList { get; set; }
    }

    public class ApplicationsList
    {
        [JsonProperty("application")]
        public List<Application>Applications { get; set; }
    }

    public class Application
    {
        [JsonProperty("access")]
        public string Access { get; set; }
        
        [JsonProperty("link_name")]
        public string LinkName { get; set; }
        
        [JsonProperty("application_name")]
        public string ApplicationName { get; set; }
        
        [JsonProperty("created_time")]
        public string CreatedTime { get; set; }
    }
}