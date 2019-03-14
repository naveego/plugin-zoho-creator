using System;
using PluginZohoCreator.Helper;
using Xunit;

namespace PluginZohoCreatorTest.Helper
{
    public class SettingsTest
    {
        [Fact]
        public void ValidateTest()
        {
            // setup
            var settings = new Settings {Token = "refresh" };
            
            // act
            settings.Validate();

            // assert
        }
        
        [Fact]
        public void ValidateNullTokenTest()
        {
            // setup
            var settings = new Settings {Token = null };
            
            // act
            Exception e  = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("the Token property must be set", e.Message);
        }
    }
}