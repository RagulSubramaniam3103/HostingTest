using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EWOMS_ClassLibrary.DataIntegration;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EWOMS_CoreAPI.Controller
{
    [Route("api/admin/comics")]
    [ApiController]
    [Authorize]
    public class ComicsController : ControllerBase
    {
        private readonly string ComicsRootPath;
        private readonly UserManager<MasterUser> _userManager;

        public ComicsController(UserManager<MasterUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            ComicsRootPath = configuration["ComicsRootPath"] ?? @"C:\Users\LENOVO\Desktop\Test\Final Full";
        }

        // GET: api/admin/comics/user-access-list
        [HttpGet("user-access-list")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserAccessList()
        {
            try
            {
                var users = await _userManager.Users
                    .OrderBy(user => user.FullName ?? user.UserName)
                    .Select(user => new
                    {
                        userId = user.Id,
                        userName = user.UserName,
                        fullName = user.FullName,
                        email = user.Email,
                        profileImage = user.ProfileImage,
                        comicAccess = user.ComicAccess,
                        isActive = user.IsActive
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to load comic access users", Error = ex.Message });
            }
        }

        // POST: api/admin/comics/grant-access
        [HttpPost("grant-access")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GrantAccess([FromBody] ComicAccessRequest? request, [FromQuery] string? userId)
        {
            return await SetComicAccess(request?.UserId ?? userId, true, "Comic access granted successfully.");
        }

        // POST: api/admin/comics/revoke-access
        [HttpPost("revoke-access")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RevokeAccess([FromBody] ComicAccessRequest? request, [FromQuery] string? userId)
        {
            return await SetComicAccess(request?.UserId ?? userId, false, "Comic access revoked successfully.");
        }

        private async Task<IActionResult> SetComicAccess(string? userId, bool hasAccess, string successMessage)
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

            user.ComicAccess = hasAccess;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "Failed to update comic access.",
                    Errors = result.Errors.Select(error => error.Description)
                });
            }

            return Ok(new
            {
                Message = successMessage,
                userId = user.Id,
                comicAccess = user.ComicAccess
            });
        }

        // GET: api/admin/comics/folders
        [HttpGet("folders")]
        public async Task<IActionResult> GetFolders()
        {
            try
            {
                if (!await CanReadComicsAsync())
                {
                    return Forbid();
                }

                if (!Directory.Exists(ComicsRootPath))
                {
                    return NotFound(new { Message = $"Comics root directory not found at: {ComicsRootPath}" });
                }

                var directories = Directory.GetDirectories(ComicsRootPath);
                var folderNames = directories.Select(d => Path.GetFileName(d)).OrderBy(name => name).ToList();

                return Ok(folderNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to list folders", Error = ex.Message });
            }
        }

        // GET: api/admin/comics/folder-images?folderName=xxx
        [HttpGet("folder-images")]
        public async Task<IActionResult> GetFolderImages([FromQuery] string folderName)
        {
            try
            {
                if (!await CanReadComicsAsync())
                {
                    return Forbid();
                }

                if (string.IsNullOrEmpty(folderName))
                {
                    return BadRequest("Folder name is required.");
                }

                // Security: Avoid directory traversal by taking only the file name
                var safeFolderName = Path.GetFileName(folderName);
                var folderPath = Path.Combine(ComicsRootPath, safeFolderName);

                if (!Directory.Exists(folderPath))
                {
                    return NotFound(new { Message = $"Directory not found: {folderName}" });
                }

                var files = Directory.GetFiles(folderPath)
                    .Where(file => new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }
                        .Contains(Path.GetExtension(file).ToLower()))
                    .Select(file => Path.GetFileName(file))
                    .ToList();

                // Sort numerically if possible, e.g. "01.jpg", "02.jpg"
                var sortedFiles = files.OrderBy(name => {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(name);
                    if (int.TryParse(nameWithoutExt, out int val))
                    {
                        return val;
                    }
                    return int.MaxValue;
                }).ThenBy(name => name).ToList();

                return Ok(sortedFiles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to list folder images", Error = ex.Message });
            }
        }

        // GET: api/admin/comics/cover?folderName=xxx
        [HttpGet("cover")]
        [AllowAnonymous]
        public IActionResult GetCover([FromQuery] string folderName)
        {
            try
            {
                if (string.IsNullOrEmpty(folderName))
                {
                    return BadRequest("Folder name is required.");
                }

                var safeFolderName = Path.GetFileName(folderName);
                var folderPath = Path.Combine(ComicsRootPath, safeFolderName);

                if (!Directory.Exists(folderPath))
                {
                    return NotFound(new { Message = $"Directory not found: {folderName}" });
                }

                var files = Directory.GetFiles(folderPath)
                    .Where(file => new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }
                        .Contains(Path.GetExtension(file).ToLower()))
                    .Select(file => Path.GetFileName(file))
                    .ToList();

                if (files.Count == 0)
                {
                    return NotFound("No images found in folder.");
                }

                var sortedFiles = files.OrderBy(name => {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(name);
                    if (int.TryParse(nameWithoutExt, out int val))
                    {
                        return val;
                    }
                    return int.MaxValue;
                }).ThenBy(name => name).ToList();

                var coverFile = sortedFiles.First();
                var filePath = Path.Combine(folderPath, coverFile);

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
                return StatusCode(500, new { Message = "Error retrieving cover", Error = ex.Message });
            }
        }

        // GET: api/admin/comics/image?folderName=xxx&fileName=yyy
        [HttpGet("image")]
        [AllowAnonymous] // Allow standard browser img src loading without headers
        public IActionResult GetImage([FromQuery] string folderName, [FromQuery] string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("Folder name and File name are required.");
                }

                var safeFolderName = Path.GetFileName(folderName);
                var safeFileName = Path.GetFileName(fileName);
                var filePath = Path.Combine(ComicsRootPath, safeFolderName, safeFileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Image not found.");
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

        private async Task<bool> CanReadComicsAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                return true;
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var user = await _userManager.FindByIdAsync(userId);
            return user?.ComicAccess == true;
        }
    }

    public class ComicAccessRequest
    {
        public string? UserId { get; set; }
    }
}
