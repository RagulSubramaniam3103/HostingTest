using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EWOMS_ClassLibrary.DataIntegration;
using EWOMS_ClassLibrary.DataControlled;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EWOMS_CoreAPI.Controller
{
    [Route("api/admin/private-videos")]
    [ApiController]
    [Authorize]
    public class PrivateVideoController : ControllerBase
    {
        private readonly string VideosRootPath;
        private readonly string BlurStatesFilePath;
        private readonly UserManager<MasterUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public PrivateVideoController(UserManager<MasterUser> userManager, ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            var configPath = configuration["VideosRootPath"];
            VideosRootPath = (!string.IsNullOrEmpty(configPath) && Directory.Exists(configPath))
                ? configPath
                : Path.Combine(Directory.GetCurrentDirectory(), "PrivateVideos");
            if (!Directory.Exists(VideosRootPath))
            {
                try { Directory.CreateDirectory(VideosRootPath); } catch { }
            }
            BlurStatesFilePath = Path.Combine(VideosRootPath, "video_blur_states.json");
            EnsurePrivateVideoAccessColumnExists();
        }

        private void EnsurePrivateVideoAccessColumnExists()
        {
            try
            {
                string sql = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EWO_MasterUser]') AND name = 'PrivateVideoAccess')
                    BEGIN
                        ALTER TABLE [dbo].[EWO_MasterUser] ADD [PrivateVideoAccess] BIT NOT NULL DEFAULT 0;
                    END";
                _dbContext.Database.ExecuteSqlRaw(sql);
            }
            catch { /* Ignore if already exists */ }
        }

        // GET: api/admin/private-videos/user-access-list
        [HttpGet("user-access-list")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserAccessList()
        {
            try
            {
                var users = await _userManager.Users
                    .OrderBy(user => user.FullName ?? user.UserName)
                    .ToListAsync();

                var userList = new List<object>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var primaryRole = roles.FirstOrDefault() ?? "User";

                    userList.Add(new
                    {
                        userId = user.Id,
                        userName = user.UserName,
                        fullName = user.FullName,
                        email = user.Email,
                        profileImage = user.ProfileImage,
                        privateVideoAccess = user.PrivateVideoAccess,
                        isActive = user.IsActive,
                        role = primaryRole
                    });
                }

                return Ok(userList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to load private video access users", Error = ex.Message });
            }
        }

        // POST: api/admin/private-videos/grant-access
        [HttpPost("grant-access")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GrantAccess([FromBody] PrivateVideoAccessRequest? request, [FromQuery] string? userId)
        {
            return await SetPrivateVideoAccess(request?.UserId ?? userId, true, "Private video access granted successfully.");
        }

        // POST: api/admin/private-videos/revoke-access
        [HttpPost("revoke-access")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RevokeAccess([FromBody] PrivateVideoAccessRequest? request, [FromQuery] string? userId)
        {
            return await SetPrivateVideoAccess(request?.UserId ?? userId, false, "Private video access revoked successfully.");
        }

        private async Task<IActionResult> SetPrivateVideoAccess(string? userId, bool hasAccess, string successMessage)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { Message = "UserId is required." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            user.PrivateVideoAccess = hasAccess;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "Failed to update private video access.",
                    Errors = result.Errors.Select(error => error.Description)
                });
            }

            return Ok(new
            {
                Message = successMessage,
                userId = user.Id,
                privateVideoAccess = user.PrivateVideoAccess
            });
        }

        private Dictionary<string, bool> ReadBlurStates()
        {
            try
            {
                if (System.IO.File.Exists(BlurStatesFilePath))
                {
                    var json = System.IO.File.ReadAllText(BlurStatesFilePath);
                    return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(json) ?? new Dictionary<string, bool>();
                }
            }
            catch { }
            return new Dictionary<string, bool>();
        }

        private void WriteBlurStates(Dictionary<string, bool> states)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(states, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                var dir = Path.GetDirectoryName(BlurStatesFilePath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                System.IO.File.WriteAllText(BlurStatesFilePath, json);
            }
            catch { }
        }

        // GET: api/admin/private-videos/videos
        [HttpGet("videos")]
        public async Task<IActionResult> GetVideos()
        {
            try
            {
                if (!await CanReadPrivateVideosAsync())
                {
                    return Forbid();
                }

                if (!Directory.Exists(VideosRootPath))
                {
                    Directory.CreateDirectory(VideosRootPath);
                }

                var files = Directory.GetFiles(VideosRootPath)
                    .Where(file => new[] { ".mp4", ".webm", ".avi", ".mkv", ".mov" }
                        .Contains(Path.GetExtension(file).ToLower()))
                    .Select(file => Path.GetFileName(file))
                    .ToList();

                var sortedFiles = files.OrderBy(name => {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(name);
                    if (int.TryParse(nameWithoutExt, out int val))
                    {
                        return val;
                    }
                    return int.MaxValue;
                }).ThenBy(name => name).ToList();

                var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
                var blurStates = ReadBlurStates();
                var result = sortedFiles.Select(name => new
                {
                    name = name,
                    isBlurred = blurStates.TryGetValue(name, out bool val) ? val : true
                });

                if (!isAdminOrManager)
                {
                    result = result.Where(x => !x.isBlurred);
                }

                return Ok(result.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to list private videos", Error = ex.Message });
            }
        }

        public class BlurStateUpdateRequest
        {
            public string? FileName { get; set; }
            public bool IsBlurred { get; set; }
        }

        // POST: api/admin/private-videos/blur-state
        [HttpPost("blur-state")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateBlurState([FromBody] BlurStateUpdateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.FileName))
                {
                    return BadRequest("FileName is required.");
                }

                var blurStates = ReadBlurStates();
                blurStates[request.FileName] = request.IsBlurred;
                WriteBlurStates(blurStates);

                return Ok(new { Message = "Blur state updated successfully.", fileName = request.FileName, isBlurred = request.IsBlurred });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to update blur state", Error = ex.Message });
            }
        }

        public class BulkBlurStateUpdateRequest
        {
            public List<string>? FileNames { get; set; }
            public bool IsBlurred { get; set; }
        }

        // POST: api/admin/private-videos/bulk-blur-state
        [HttpPost("bulk-blur-state")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateBulkBlurState([FromBody] BulkBlurStateUpdateRequest request)
        {
            try
            {
                if (request?.FileNames == null || request.FileNames.Count == 0)
                {
                    return BadRequest("FileNames are required.");
                }

                var blurStates = ReadBlurStates();
                foreach (var fileName in request.FileNames)
                {
                    blurStates[fileName] = request.IsBlurred;
                }
                WriteBlurStates(blurStates);

                return Ok(new { Message = "Bulk blur states updated successfully.", count = request.FileNames.Count, isBlurred = request.IsBlurred });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to bulk update blur states", Error = ex.Message });
            }
        }

        // GET: api/admin/private-videos/video?fileName=xxx
        [HttpGet("video")]
        public async Task<IActionResult> GetVideo([FromQuery] string fileName)
        {
            try
            {
                if (!await CanReadPrivateVideosAsync())
                {
                    return Forbid();
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("File name is required.");
                }

                var safeFileName = Path.GetFileName(fileName);
                var filePath = Path.Combine(VideosRootPath, safeFileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Video not found.");
                }

                var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
                if (!isAdminOrManager)
                {
                    var blurStates = ReadBlurStates();
                    if (!blurStates.TryGetValue(safeFileName, out bool isBlurred) || isBlurred)
                    {
                        return Forbid();
                    }
                }

                var extension = Path.GetExtension(filePath).ToLower();
                string contentType = extension switch
                {
                    ".mp4" => "video/mp4",
                    ".webm" => "video/webm",
                    ".avi" => "video/x-msvideo",
                    ".mkv" => "video/x-matroska",
                    ".mov" => "video/quicktime",
                    _ => "application/octet-stream"
                };

                // Premium seekable range support
                return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving video", Error = ex.Message });
            }
        }

        // GET: api/admin/private-videos/next-name
        [HttpGet("next-name")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult GetNextName([FromQuery] string? extension)
        {
            try
            {
                var ext = (extension ?? ".mp4").ToLower();
                if (!ext.StartsWith(".")) ext = "." + ext;

                if (!Directory.Exists(VideosRootPath))
                {
                    return Ok(new { nextName = "1" + ext });
                }

                var files = Directory.GetFiles(VideosRootPath)
                    .Where(file => new[] { ".mp4", ".webm", ".avi", ".mkv", ".mov" }
                        .Contains(Path.GetExtension(file).ToLower()))
                    .Select(file => Path.GetFileName(file))
                    .ToList();

                if (files.Count == 0)
                {
                    return Ok(new { nextName = "1" + ext });
                }

                int highestNumeric = 0;
                foreach (var file in files)
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    if (int.TryParse(nameWithoutExt, out int val))
                    {
                        highestNumeric = Math.Max(highestNumeric, val);
                    }
                }

                return Ok(new { nextName = (highestNumeric + 1).ToString() + ext });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to calculate next name", Error = ex.Message });
            }
        }

        // POST: api/admin/private-videos/upload
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UploadVideo([FromForm] IFormFile file, [FromForm] string fileName)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return BadRequest("Filename is required.");
                }

                var safeFileName = Path.GetFileName(fileName);
                var ext = Path.GetExtension(safeFileName).ToLower();
                if (!new[] { ".mp4", ".webm", ".avi", ".mkv", ".mov" }.Contains(ext))
                {
                    return BadRequest("Invalid video format.");
                }

                if (!Directory.Exists(VideosRootPath))
                {
                    Directory.CreateDirectory(VideosRootPath);
                }

                var filePath = Path.Combine(VideosRootPath, safeFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var blurStates = ReadBlurStates();
                blurStates[safeFileName] = true;
                WriteBlurStates(blurStates);

                return Ok(new { Message = "Video uploaded successfully.", fileName = safeFileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Upload failed", Error = ex.Message });
            }
        }

        private async Task<MasterUser?> GetCurrentUserAsync()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return null;
            return await _userManager.FindByIdAsync(userId);
        }

        private async Task<bool> CanReadPrivateVideosAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                return true;
            }

            var user = await GetCurrentUserAsync();
            return user?.PrivateVideoAccess == true;
        }
    }

    public class PrivateVideoAccessRequest
    {
        public string? UserId { get; set; }
    }
}
