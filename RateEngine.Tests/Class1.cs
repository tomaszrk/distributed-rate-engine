using Aspire.Hosting.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RateEngine.Tests
{
    public class IntegrationTest
    {
        [Fact]
        public async Task GetWebResourceRootReturnsOkStatusCode()
        {
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.RateEngine_AppHost>();
            await using var app = await appHost.BuildAsync();
            await app.StartAsync();
            var client = app.CreateHttpClient("apiservice");

            var newHotel = new
            {
                Name = "Integration Test Hotel",
                City = "Test City",
                Stars = 5,
                Street = "123 Test St",
                ZipCode = "12345",
                Country = "Testland"
            };

            var createResponse = await client.PostAsJsonAsync("/hotels", newHotel);
            Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);
            var createdHotel = await createResponse.Content.ReadFromJsonAsync<HotelResponse>();
            Assert.NotNull(createdHotel);
            var hotelId = createdHotel.Id;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool found = false;

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(10)) // Give it 10 seconds max
            {
                var getResponse = await client.GetAsync($"/hotels/{hotelId}");

                if (getResponse.StatusCode == HttpStatusCode.OK)
                {
                    found = true;
                    break; // Success!
                }

                // Wait 500ms before trying again
                await Task.Delay(500);
            }
            Assert.True(found, "Hotel was not found in SQL after 10 seconds. Worker failed?");
        }

        record HotelResponse(Guid Id, string Name, string City);
    }
}
