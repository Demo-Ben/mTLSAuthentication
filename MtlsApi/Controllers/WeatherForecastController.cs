using Microsoft.AspNetCore.Mvc;
using MtlsApi.Controllers;

namespace MtlsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }

    public class WeatherForecast
    {
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string? Summary { get; set; }
    }
}



//// 1. Check if the token is empty BEFORE doing anything else
//if (string.IsNullOrWhiteSpace(JwtState.Token))
//{
//    errorMessage = "Access Denied: You are not logged in or your session has expired.";
//    return; // Stop execution immediately; do not call the API
//}

//// 2. Token exists, safe to build the client
//var client = _clientFactory.CreateClient("ApiClient");

//client.DefaultRequestHeaders.Authorization =
//                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JwtState.Token);

//// 3. Make the API call
//var response = await client.GetAsync("api/WeatherForecast");

//if (response.IsSuccessStatusCode)
//{
//    _forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
//}
//// 4. Handle server-side errors (like an expired token the server rejects)
//else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
//{
//    errorMessage = "Your session has expired. Please log in again.";
//}
//else
//{
//    errorMessage = $"Server Error: Unable to fetch data (Status code: {response.StatusCode}).";
//}