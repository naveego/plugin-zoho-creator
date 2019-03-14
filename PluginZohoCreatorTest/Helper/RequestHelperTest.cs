using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PluginZohoCreator.Helper;
using RichardSzalay.MockHttp;
using Xunit;

namespace PluginZohoCreatorTest.Helper
{
    public class RequestHelperTest
    {
        [Fact]
        public async Task GetAsyncTest()
        {
            // setup
            var mockHttp = new MockHttpMessageHandler();
            
            mockHttp.When("https://mockrequest.net?authtoken=mocktoken")
                .Respond("application/json", "success");

            var requestHelper = new RequestHelper(new Settings{ Token = "mocktoken" }, mockHttp.ToHttpClient());
            
            // act
            var response = await requestHelper.GetAsync("https://mockrequest.net");

            // assert
            Assert.Equal("success", await response.Content.ReadAsStringAsync());
        }
        
        [Fact]
        public async Task GetAsyncWithRequestExceptionTest()
        {
            // setup
            var mockHttp = new MockHttpMessageHandler();
            
            mockHttp.When("https://mockrequest.net?authtoken=mocktoken")
                .Throw(new Exception("bad stuff"));

            var requestHelper = new RequestHelper(new Settings{ Token = "mocktoken" }, mockHttp.ToHttpClient());
            
            // act
            Exception e  = await Assert.ThrowsAsync<Exception>(async () => await requestHelper.GetAsync("https://mockrequest.net"));

            // assert
            Assert.Contains("bad stuff", e.Message);
        }
    }
}