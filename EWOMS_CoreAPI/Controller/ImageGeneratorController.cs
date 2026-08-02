using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EWOMS_CoreAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Allows anonymous access to fix 401 errors in Swagger testing
    public class ImageGeneratorController : ControllerBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public class ImageGenerateRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("styleId")]
            public int StyleId { get; set; } = 4;

            [JsonPropertyName("size")]
            public string Size { get; set; } = "1-1";
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateImage([FromBody] ImageGenerateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Prompt))
            {
                return BadRequest(new { Message = "Prompt cannot be empty" });
            }

            try
            {
                var escapedPrompt = req.Prompt.Replace("\"", "\\\"");
                var jsonContent = $"{{\"prompt\":\"{escapedPrompt}\",\"style_id\":{req.StyleId},\"size\":\"{req.Size}\"}}";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://ai-text-to-image-generator-flux-free-api.p.rapidapi.com/aaaaaaaaaaaaaaaaaiimagegenerator/quick.php"),
                    Headers =
                    {
                        { "x-rapidapi-key", "0de8f5e6famshd6da39ce6ca8095p15680bjsn0e6db5b5b196" },
                        { "x-rapidapi-host", "ai-text-to-image-generator-flux-free-api.p.rapidapi.com" },
                    },
                    Content = new StringContent(jsonContent)
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/json")
                        }
                    }
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
                return StatusCode(500, new { Message = "Failed to generate image from AI API", Error = ex.Message });
            }
        }
    }
}
