using System;
using System.Collections.Generic;

namespace PluginZohoCreator.Helper
{
    public class Settings
    {
        public string Token { get; set; }
        public List<CustomUrlObject> CustomSchemaList { get; set; }

        /// <summary>
        /// Validates the settings input object
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Validate()
        {
            if (String.IsNullOrEmpty(Token))
            {
                throw new Exception("the Token property must be set");
            }
        }
    }

    public class CustomUrlObject
    {
        public string ApplicationOwner { get; set; }
        public string ApplicationName { get; set; }
        public string FormName { get; set; }
        public string ViewName { get; set; }
    }
}