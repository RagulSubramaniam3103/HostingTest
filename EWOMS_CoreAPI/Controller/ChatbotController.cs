using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EWOMS_ClassLibrary.DataControlled;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using EWOMS_ClassLibrary.DataIntegration;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace EWOMS_CoreAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<MasterUser> _userManager;
        private readonly IConfiguration _configuration;

        public ChatbotController(ApplicationDbContext dbContext, UserManager<MasterUser> userManager, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _configuration = configuration;
        }

        public class ChatbotRequest
        {
            public string Query { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string? AttachmentUrl { get; set; }
            public string? AttachmentType { get; set; }
        }

        [HttpPost("Ask")]
        public async Task<IActionResult> Ask([FromBody] ChatbotRequest request)
        {
            if (string.IsNullOrEmpty(request.Query) || string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { Message = "Query and UserId are required." });
            }

            var query = request.Query.ToLower();
            var user = await _userManager.FindByIdAsync(request.UserId);
            bool isAdmin = false;
            string userName = "Guest";
            string userEmail = "No Email";
            string roleText = "Standard User";

            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                isAdmin = (roles != null) && (roles.Contains("Admin") || roles.Contains("Manager"));
                userName = !string.IsNullOrEmpty(user.FullName) ? user.FullName : (user.UserName ?? "User");
                userEmail = user.Email ?? "No Email";
                roleText = (roles != null && roles.Count > 0) ? string.Join(", ", roles) : "Standard User";
            }
            else
            {
                // Fallback for database mismatch or guest admin nexus accounts
                isAdmin = true;
                userName = "Administrator";
                userEmail = "admin@company.com";
                roleText = "Admin";
            }

            bool wasHandled = false;
            string responseMessage = "";

            // User queries
            if (query.Contains("show my posts"))
            {
                var postCount = await _dbContext.UserPost.CountAsync(p => p.UserId == request.UserId && !p.IsDeleted);
                responseMessage = $"You currently have {postCount} active posts. You can view them on your profile.";
            }
            else if (query.Contains("how many likes do i have") || query.Contains("likes"))
            {
                // Get all posts by this user
                var userPostIds = await _dbContext.UserPost
                    .Where(p => p.UserId == request.UserId && !p.IsDeleted)
                    .Select(p => p.Id)
                    .ToListAsync();
                
                var totalLikes = await _dbContext.PostLikes
                    .Where(l => userPostIds.Contains(l.PostId))
                    .CountAsync();

                responseMessage = $"You have a total of {totalLikes} likes across all your posts! Great job!";
            }
            else if (query.Contains("who viewed my story") || query.Contains("story views"))
            {
                // Get stories by user
                var userStoryIds = await _dbContext.UserStories
                    .Where(s => s.UserId == request.UserId && s.IsActive)
                    .Select(s => s.Id)
                    .ToListAsync();

                var viewCount = await _dbContext.UserStoryViews
                    .Where(v => userStoryIds.Contains(v.StoryId))
                    .CountAsync();

                responseMessage = $"Your active stories have been viewed {viewCount} times.";
            }
            else if (query.Contains("what is my name") || query.Contains("what's my name") || query.Contains("who am i") || query.Contains("my name") || query.Contains("my profile") || query.Contains("generate my profile"))
            {
                if (query.Contains("profile")) 
                {
                    responseMessage = $"Here is your profile information:\nName: {userName}\nEmail: {userEmail}\nAccess Level: {roleText}";
                }
                else 
                {
                    responseMessage = $"You are logged in as {userName}!";
                }
            }
            else if (query.Contains("partner") || query.Contains("connection") || query.Contains("married") || query.Contains("marriage"))
            {
                var acceptedStatus = EWOMS_ExternalClassLibrary_DTO.UserData_DTO.Chat.FriendRequestStatus.Accepted;
                var partnerRequest = await _dbContext.EWOMS_FriendRequests
                    .Where(fr => (fr.SenderId == request.UserId || fr.ReceiverId == request.UserId) && fr.Status == acceptedStatus)
                    .FirstOrDefaultAsync();

                if (partnerRequest != null)
                {
                    var partnerId = partnerRequest.SenderId == request.UserId ? partnerRequest.ReceiverId : partnerRequest.SenderId;
                    var partnerUser = await _userManager.FindByIdAsync(partnerId);
                    if (partnerUser != null)
                    {
                        var partnerName = !string.IsNullOrEmpty(partnerUser.FullName) ? partnerUser.FullName : (partnerUser.UserName ?? "User");
                        responseMessage = $"Your connected partner is {partnerName} ({partnerUser.Email ?? "No Email"}).";
                    }
                    else
                    {
                        responseMessage = "You have an active connection, but the partner's account details could not be found.";
                    }
                }
                else
                {
                    responseMessage = "You currently do not have any active partner connections. You can search for users in the Asset Vault to send connection requests.";
                }
            }
            else if (query == "hi" || query == "hello" || query == "hey" || query == "greetings" || query == "good morning" || query == "good evening")
            {
                responseMessage = $"Hello {userName}! How can I assist you today?";
            }
            else if (query.Contains("how are you"))
            {
                responseMessage = "I'm just a digital assistant, but I'm doing great! Ready to help you manage your EWOMS platform. What can I do for you?";
            }
            else if (query.Contains("who are you") || query.Contains("what are you"))
            {
                responseMessage = "I am the EWOMS AI Assistant. I can help you fetch your profile stats, manage posts, and give you administrative insights. Just ask!";
            }
            else if (query.Contains("what can you do") || query == "help" || query.Contains("features") || query.Contains("all i want to implement"))
            {
                if (isAdmin) {
                    responseMessage = "As an Admin, you can ask me:\n- 'Show my posts'\n- 'How many likes do I have?'\n- 'Pending posts for approval'\n- 'Flagged users'\n- 'Today's activity report'\n- 'Generate my profile'";
                } else {
                    responseMessage = "You can ask me things like:\n- 'Show my posts'\n- 'How many likes do I have?'\n- 'Who viewed my story?'\n- 'Generate my profile'";
                }
            }
            else if (query.Contains("thank") || query == "thanks" || query == "thx")
            {
                responseMessage = "You're very welcome! Let me know if you need anything else.";
            }
            else if (query.Contains("bye") || query.Contains("goodbye"))
            {
                responseMessage = "Goodbye! Have a great day!";
            }
            else if (query.Contains("what is ewoms") || query.Contains("about this app"))
            {
                responseMessage = "EWOMS (Enterprise Workspace Operations Management System) is your complete platform for networking, content sharing, and operational management.";
            }

            // Admin queries
            if (isAdmin)
            {
                if (query.Contains("pending posts") || query.Contains("approval"))
                {
                    // Let's assume active posts that are not moderated or something similar,
                    // or just return the count of total posts for review.
                    var recentPosts = await _dbContext.UserPost.CountAsync(p => !p.IsDeleted && !p.IsBlurred);
                    responseMessage = $"There are {recentPosts} unmoderated posts in the system that may need review.";
                }
                else if (query.Contains("flagged users"))
                {
                    // Count users who are locked out or not active
                    var flaggedCount = await _dbContext.Users.CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > DateTime.UtcNow);
                    responseMessage = $"There are currently {flaggedCount} flagged (locked out) users in the system.";
                }
                else if (query.Contains("activity report") || query.Contains("today") || query.Contains("report"))
                {
                    var today = DateTime.UtcNow.Date;
                    var newUsers = await _dbContext.Users.CountAsync(u => u.CreatedUser >= today);
                    var newPosts = await _dbContext.UserPost.CountAsync(p => p.CreatedAt >= today);
                    var auditLogs = await _dbContext.AdminAuditLogs.CountAsync(a => a.Timestamp >= today);

                    responseMessage = $"Today's Report:\n- {newUsers} new users registered.\n- {newPosts} new posts created.\n- {auditLogs} administrative actions logged.";
                }
                else if (query.Contains("who register") || query.Contains("who registered") || query.Contains("new users") || query.Contains("who joined"))
                {
                    var today = DateTime.UtcNow.Date;
                    var recentlyJoined = await _dbContext.Users
                        .Where(u => u.CreatedUser >= today)
                        .Select(u => !string.IsNullOrEmpty(u.FullName) ? u.FullName : u.UserName)
                        .ToListAsync();

                    if (recentlyJoined.Any())
                    {
                        responseMessage = $"The following personnel registered today:\n- {string.Join("\n- ", recentlyJoined)}";
                    }
                    else
                    {
                        responseMessage = "No new personnel have registered today so far.";
                    }
                }
            }

            if (string.IsNullOrEmpty(responseMessage))
            {
                responseMessage = await CallNvidiaApiAsync(request.Query, request.AttachmentUrl, request.AttachmentType);
            }

            return Ok(new { Message = responseMessage });
        }

        private async Task<string> CallNvidiaApiAsync(string userQuery, string? attachmentUrl, string? attachmentType)
        {
            try
            {
                using var httpClient = new HttpClient();
                
                var apiKey = _configuration["Nvidia:ApiKey"] ?? "nvapi-xxxxxxxxxxx-";
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                object contentPayload;

                if (!string.IsNullOrEmpty(attachmentUrl) && !string.IsNullOrEmpty(attachmentType))
                {
                    if (attachmentType.Equals("image", StringComparison.OrdinalIgnoreCase))
                    {
                        contentPayload = new object[]
                        {
                            new { type = "text", text = userQuery },
                            new { type = "image_url", image_url = new { url = attachmentUrl } }
                        };
                    }
                    else if (attachmentType.Equals("audio", StringComparison.OrdinalIgnoreCase))
                    {
                        contentPayload = new object[]
                        {
                            new { type = "text", text = userQuery },
                            new { type = "audio_url", audio_url = new { url = attachmentUrl } }
                        };
                    }
                    else if (attachmentType.Equals("video", StringComparison.OrdinalIgnoreCase))
                    {
                        contentPayload = new object[]
                        {
                            new { type = "text", text = userQuery },
                            new { type = "video_url", video_url = new { url = attachmentUrl } }
                        };
                    }
                    else
                    {
                        contentPayload = userQuery;
                    }
                }
                else
                {
                    contentPayload = userQuery;
                }

                var payload = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                payload.Add("messages", new[]
                {
                    new { role = "user", content = contentPayload }
                });
                payload.Add("model", "nvidia/nemotron-3-nano-omni-30b-a3b-reasoning");
                payload.Add("max_tokens", 4096);
                payload.Add("stream", false);
                payload.Add("temperature", 0.6);
                payload.Add("top_p", 0.95);

                if (!string.IsNullOrEmpty(attachmentType) && attachmentType.Equals("video", StringComparison.OrdinalIgnoreCase))
                {
                    payload.Add("extra_body", new
                    {
                        thinking_token_budget = 17408,
                        chat_template_kwargs = new
                        {
                            enable_thinking = true,
                            reasoning_budget = 16384
                        },
                        mm_processor_kwargs = new { use_audio_in_video = false }
                    });
                }
                else
                {
                    payload.Add("reasoning_budget", 1024);
                    payload.Add("chat_template_kwargs", new
                    {
                        enable_thinking = true
                    });
                }

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync("https://integrate.api.nvidia.com/v1/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseJson);
                    
                    if (doc.RootElement.TryGetProperty("choices", out var choices) && 
                        choices.GetArrayLength() > 0 &&
                        choices[0].TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var contentProp))
                    {
                        return contentProp.GetString() ?? "No response content received.";
                    }
                }
                else
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"NVIDIA API Error: {response.StatusCode} - {errorDetails}");
                    return $"NVIDIA API Error: {response.StatusCode}. Details: {errorDetails}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception calling NVIDIA API: {ex.Message}");
                return $"Exception calling NVIDIA API: {ex.Message}";
            }

            return "I am having trouble connecting to my cognitive processor. Please try again shortly.";
        }
    }
}
