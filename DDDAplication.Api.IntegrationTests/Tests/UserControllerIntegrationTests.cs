using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.API;

namespace DDDAplication.Api.IntegrationTests
{
    [TestFixture] 
    public class UserControllerIntegrationTests
    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory; 

        public UserControllerIntegrationTests()
        {
            _factory = new ApiApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [OneTimeSetUp] 
        [SetUp]
        public void SetUp()
        {
        }



        [OneTimeTearDown] 
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose(); 
        }
    }
}