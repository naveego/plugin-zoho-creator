using System;

namespace PluginZohoCreator.Helper
{
    public class Settings
    {
        public string Token { get; set; }

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
}