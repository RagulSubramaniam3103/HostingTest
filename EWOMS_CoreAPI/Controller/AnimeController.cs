using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EWOMS_CoreAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Allow anyone to search
    public class AnimeController : ControllerBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        [HttpGet("search")]
        public async Task<IActionResult> GetAnime(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? genres = null,
            [FromQuery] string? sortBy = "ranking",
            [FromQuery] string? sortOrder = "asc")
        {
            try
            {
                var queryParams = $"page={page}&size={size}";
                if (!string.IsNullOrEmpty(search)) queryParams += $"&search={Uri.EscapeDataString(search)}";
                if (!string.IsNullOrEmpty(genres)) queryParams += $"&genres={Uri.EscapeDataString(genres)}";
                if (!string.IsNullOrEmpty(sortBy)) queryParams += $"&sortBy={Uri.EscapeDataString(sortBy)}";
                if (!string.IsNullOrEmpty(sortOrder)) queryParams += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";

                var requestUri = $"https://anime-db.p.rapidapi.com/anime?{queryParams}";
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(requestUri),
                    Headers =
                    {
                        { "x-rapidapi-key", "0de8f5e6famshd6da39ce6ca8095p15680bjsn0e6db5b5b196" },
                        { "x-rapidapi-host", "anime-db.p.rapidapi.com" },
                    },
                };

                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    return Content(body, "application/json");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to query Anime DB API", Error = ex.Message });
            }
        }
    }
}
