using PluginZohoCreator.Helper;
using Xunit;

namespace PluginZohoCreatorTest.Helper
{
    public class AuthenticatorTest
    {
        [Fact]
        public void GetTokenTest()
        {
            // setup
            var auth = new Authenticator(new Settings{ Token = "mocktoken"});
            
            // act
            var token = auth.GetToken();


            // assert
            // first token is fetched
            Assert.Equal("mocktoken", token);

        }
    }
}