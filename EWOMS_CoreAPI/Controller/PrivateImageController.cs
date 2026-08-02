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
    [Route("api/admin/private-images")]
    [ApiController]
    [Authorize]
    public class PrivateImageController : ControllerBase
    {
        private readonly string ImagesRootPath;
        private readonly string BlurStatesFilePath;
        private readonly UserManager<MasterUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public PrivateImageController(UserManager<MasterUser> userManager, ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            var configPath = configuration["ImagesRootPath"];
            ImagesRootPath = (!string.IsNullOrEmpty(configPath) && Directory.Exists(configPath))
                ? configPath
                : Path.Combine(Directory.GetCurrentDirectory(), "PrivateImages");
            if (!Directory.Exists(ImagesRootPath))
            {
                try { Directory.CreateDirectory(ImagesRootPath); } catch { }
            }
            BlurStatesFilePath = Path.Combine(ImagesRootPath, "blur_states.json");
            EnsurePrivateImageAccessColumnExists();
        }

        private void EnsurePrivateImageAccessColumnExists()
        {
            try
            {
                string sql1 = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EWO_MasterUser]') AND name = 'PrivateImageAccess')
                    BEGIN
                        ALTER TABLE [dbo].[EWO_MasterUser] ADD [PrivateImageAccess] BIT NOT NULL DEFAULT 0;
                    END";
                _dbContext.Database.ExecuteSqlRaw(sql1);

                string sql2 = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EWO_MasterUser]') AND name = 'PrivateMenuPassword')
                    BEGIN
                        ALTER TABLE [dbo].[EWO_MasterUser] ADD [PrivateMenuPassword] NVARCHAR(MAX) NULL;
                    END";
                _dbContext.Database.ExecuteSqlRaw(sql2);
            }
            catch { /* Ignore if table/column already exists or not supported */ }
        }

        // GET: api/admin/private-images/user-access-list
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
                        privateImageAccess = user.PrivateImageAccess,
                        isActive = user.IsActive,
                        role = primaryRole
                    });
                }

                return Ok(userList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to load private image access users", Error = ex.Message });
            }
        }

        // POST: api/admin/private-images/grant-access
        [HttpPost("grant-access")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GrantAccess([FromBody] PrivateImageAccessRequest? request, [FromQuery] string? userId)
        {
            return await SetPrivateImageAccess(request?.UserId ?? userId, true, "Private image access granted successfully.");
        }

        // POST: api/admin/private-images/revoke-access
        [HttpPost("revoke-access")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RevokeAccess([FromBody] PrivateImageAccessRequest? request, [FromQuery] string? userId)
        {
            return await SetPrivateImageAccess(request?.UserId ?? userId, false, "Private image access revoked successfully.");
        }

        private async Task<IActionResult> SetPrivateImageAccess(string? userId, bool hasAccess, string successMessage)
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

            user.PrivateImageAccess = hasAccess;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "Failed to update private image access.",
                    Errors = result.Errors.Select(error => error.Description)
                });
            }

            return Ok(new
            {
                Message = successMessage,
                userId = user.Id,
                privateImageAccess = user.PrivateImageAccess
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

        // GET: api/admin/private-images/images
        [HttpGet("images")]
        public async Task<IActionResult> GetImages()
        {
            try
            {
                if (!await CanReadPrivateImagesAsync())
                {
                    return Forbid();
                }

                if (!Directory.Exists(ImagesRootPath))
                {
                    try { Directory.CreateDirectory(ImagesRootPath); } catch { }
                    return Ok(new List<object>());
                }

                var files = Directory.GetFiles(ImagesRootPath)
                    .Where(file => new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }
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
                    isBlurred = blurStates.TryGetValue(name, out bool val) ? val : true // Default to true (blurred)
                });

                if (!isAdminOrManager)
                {
                    result = result.Where(x => !x.isBlurred);
                }

                return Ok(result.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to list private images", Error = ex.Message });
            }
        }

        public class VerifyPasswordRequest
        {
            public string Password { get; set; } = string.Empty;
        }

        public class SetPasswordRequest
        {
            public string Password { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public class ChangePrivatePasswordRequest
        {
            public string OldPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmNewPassword { get; set; } = string.Empty;
        }

        // GET: api/admin/private-images/password-status
        [HttpGet("password-status")]
        public async Task<IActionResult> GetPasswordStatus()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized();

                var isSet = !string.IsNullOrEmpty(user.PrivateMenuPassword);
                return Ok(new { isPasswordSet = isSet });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to retrieve status", Error = ex.Message });
            }
        }

        // POST: api/admin/private-images/verify-password
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Password is required.");
                }

                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized();

                if (string.IsNullOrEmpty(user.PrivateMenuPassword))
                {
                    return BadRequest(new { Message = "Private menu password is not set yet." });
                }

                if (user.PrivateMenuPassword != request.Password)
                {
                    return BadRequest(new { Message = "Invalid security clearance password." });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to verify security clearance", Error = ex.Message });
            }
        }

        // POST: api/admin/private-images/set-password
        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Password is required.");
                }

                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest("Passwords do not match.");
                }

                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized();

                if (!string.IsNullOrEmpty(user.PrivateMenuPassword))
                {
                    return BadRequest("Password already set. Use change password instead.");
                }

                user.PrivateMenuPassword = request.Password;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest("Failed to save password.");
                }

                return Ok(new { Message = "Password set successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to set password", Error = ex.Message });
            }
        }

        // POST: api/admin/private-images/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePrivatePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest("All password fields are required.");
                }

                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    return BadRequest("New passwords do not match.");
                }

                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized();

                if (user.PrivateMenuPassword != request.OldPassword)
                {
                    return BadRequest("Incorrect old password.");
                }

                user.PrivateMenuPassword = request.NewPassword;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest("Failed to change password.");
                }

                return Ok(new { Message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to change password", Error = ex.Message });
            }
        }

        public class BlurStateUpdateRequest
        {
            public string? FileName { get; set; }
            public bool IsBlurred { get; set; }
        }

        // POST: api/admin/private-images/blur-state
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

        // POST: api/admin/private-images/bulk-blur-state
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

        // GET: api/admin/private-images/image?fileName=xxx
        [HttpGet("image")]
        public async Task<IActionResult> GetImage([FromQuery] string fileName)
        {
            try
            {
                if (!await CanReadPrivateImagesAsync())
                {
                    return Forbid();
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("File name is required.");
                }

                var safeFileName = Path.GetFileName(fileName);
                var filePath = Path.Combine(ImagesRootPath, safeFileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Image not found.");
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
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };

                Response.Headers["Cache-Control"] = "public, max-age=604800";
                return PhysicalFile(filePath, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving image", Error = ex.Message });
            }
        }

        // GET: api/admin/private-images/next-name
        [HttpGet("next-name")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult GetNextName([FromQuery] string? extension)
        {
            try
            {
                var ext = (extension ?? ".jpg").ToLower();
                if (!ext.StartsWith(".")) ext = "." + ext;

                if (!Directory.Exists(ImagesRootPath))
                {
                    return Ok(new { nextName = "1" + ext });
                }

                var files = Directory.GetFiles(ImagesRootPath)
                    .Where(file => new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }
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

        // POST: api/admin/private-images/upload
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] string fileName)
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
                
                // Ensure extension is valid
                var ext = Path.GetExtension(safeFileName).ToLower();
                if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                {
                    return BadRequest("Invalid image format.");
                }

                if (!Directory.Exists(ImagesRootPath))
                {
                    Directory.CreateDirectory(ImagesRootPath);
                }

                var filePath = Path.Combine(ImagesRootPath, safeFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Register in blur states as true (default to blurred)
                var blurStates = ReadBlurStates();
                blurStates[safeFileName] = true;
                WriteBlurStates(blurStates);

                return Ok(new { Message = "Image uploaded successfully.", fileName = safeFileName });
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

        private async Task<bool> CanReadPrivateImagesAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                return true;
            }

            var user = await GetCurrentUserAsync();
            return user?.PrivateImageAccess == true;
        }
    }

    public class PrivateImageAccessRequest
    {
        public string? UserId { get; set; }
    }
}
