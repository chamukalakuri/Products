using Products.Api.IntegrationTest.Helper;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Products.Api.IntegrationTest
{
        /// <summary>
        /// Integration tests for the health check endpoint
        /// </summary>
        public class HealthCheckIntegrationTests : IClassFixture<CustomWebApplicationFactory>
        {
            private readonly CustomWebApplicationFactory _factory;
            private readonly HttpClient _client;

            public HealthCheckIntegrationTests(CustomWebApplicationFactory factory)
            {
                _factory = factory;
                _client = _factory.CreateClient();
            }

            [Fact]
            public async Task HealthCheck_ShouldBeAccessible_WithoutAuthentication()
            {
                // Act
                var response = await _client.GetAsync("/health");

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("healthy", content.ToLower());
            }

            [Fact]
            public async Task MultipleHealthCheckRequests_ShouldReturnSuccess()
            {
                // Arrange
                const int requestCount = 5;

                // Act & Assert
                for (int i = 0; i < requestCount; i++)
                {
                    var response = await _client.GetAsync("/health");
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    // Small delay to avoid potential rate limiting
                    await Task.Delay(100);
                }
            }
        }
    }