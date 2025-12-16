namespace TFCiclo.Tests
{
    [TestClass]
    public sealed class HealthEndpointTests
    {
        private readonly HttpClient _client = new HttpClient();

        [TestMethod]
        public async Task HealthEndpoint_ShouldReturn200OK()
        {
            // 🔹 Cambia el puerto si tu API usa otro (ver launchSettings.json)
            string url = "https://localhost:7008/health";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode, $"El endpoint devolvió {(int)response.StatusCode}");
        }
    }
}
