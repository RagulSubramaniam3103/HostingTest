using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EWOMS_ClassLibrary.DataIntegration;
using EWOMS_ClassLibrary.DataControlled;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EWOMS_CoreAPI.Controller
{
    [Route("api/admin/menu-access")]
    [ApiController]
    [Authorize]
    public class MenuAccessController : ControllerBase
    {
        private readonly UserManager<MasterUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public MenuAccessController(UserManager<MasterUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: api/admin/menu-access/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserMenuPermissions(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = (roles.FirstOrDefault() ?? "User").ToLower();

                // Define all system menus
                var systemMenus = new List<string>
                {
                    "Dashboard",
                    "Directory",
                    "Intelligence Hub",
                    "Add User",
                    "Security",
                    "Moderation",
                    "Intel Recovery",
                    "Security Audit",
                    "Construction Ledger",
                    "Publish Post",
                    "Share Story",
                    "Community",
                    "Comics",
                    "Private Gallery",
                    "Private Video",
                    "Manage Access",
                    "Manage Media",
                    "Profile",
                    "Settings",
                    "My Intelligence",
                    "Personal Assets",
                    "New Post",
                    "Messenger",
                    "Notes",
                    "Private Password",
                    "AI Command Center"
                };

                // Fetch custom permissions from database
                var dbPermissions = await _dbContext.MenuAccesses
                    .Where(m => m.UserId == userId)
                    .ToListAsync();

                var resultList = new List<object>();

                foreach (var menu in systemMenus)
                {
                    var customPerm = dbPermissions.FirstOrDefault(p => p.MenuName.Equals(menu, StringComparison.OrdinalIgnoreCase));
                    
                    bool hasAccess;
                    bool isPrivate;

                    if (primaryRole == "admin" || primaryRole == "manager")
                    {
                        hasAccess = true;
                        isPrivate = customPerm?.IsPrivate ?? (menu.Equals("Private Gallery", StringComparison.OrdinalIgnoreCase) || menu.Equals("Private Video", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (customPerm != null)
                    {
                        hasAccess = customPerm.HasAccess;
                        isPrivate = customPerm.IsPrivate;
                    }
                    else
                    {
                        // Standard users default allowed set
                        var allowedDefaultUserMenus = new[]
                        {
                            "Dashboard", "Community", "Comics", "Profile", "Settings", "Messenger", "Notes"
                        };
                        hasAccess = allowedDefaultUserMenus.Contains(menu);
                        isPrivate = menu.Equals("Private Gallery", StringComparison.OrdinalIgnoreCase) || menu.Equals("Private Video", StringComparison.OrdinalIgnoreCase);
                    }

                    resultList.Add(new
                    {
                        menuName = menu,
                        hasAccess = hasAccess,
                        isPrivate = isPrivate
                    });
                }

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to load menu permissions", Error = ex.Message });
            }
        }

        public class SaveMenuPermissionsRequest
        {
            public string UserId { get; set; } = string.Empty;
            public List<MenuPermissionDto> MenuPermissions { get; set; } = new();
        }

        public class MenuPermissionDto
        {
            public string MenuName { get; set; } = string.Empty;
            public bool HasAccess { get; set; }
            public bool IsPrivate { get; set; }
        }

        // POST: api/admin/menu-access/save
        [HttpPost("save")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SaveMenuPermissions([FromBody] SaveMenuPermissionsRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest("UserId is required.");
                }

                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                // Delete existing ones for this user
                var existing = await _dbContext.MenuAccesses
                    .Where(m => m.UserId == request.UserId)
                    .ToListAsync();

                if (existing.Any())
                {
                    _dbContext.MenuAccesses.RemoveRange(existing);
                }

                // Insert new ones
                foreach (var dto in request.MenuPermissions)
                {
                    var item = new MenuAccess
                    {
                        UserId = request.UserId,
                        MenuName = dto.MenuName,
                        HasAccess = dto.HasAccess,
                        IsPrivate = dto.IsPrivate
                    };

                    // Synchronize the legacy column PrivateImageAccess for backward compatibility
                    if (dto.MenuName.Equals("Private Gallery", StringComparison.OrdinalIgnoreCase))
                    {
                        user.PrivateImageAccess = dto.HasAccess;
                    }

                    if (dto.MenuName.Equals("Private Video", StringComparison.OrdinalIgnoreCase))
                    {
                        user.PrivateVideoAccess = dto.HasAccess;
                    }

                    if (dto.MenuName.Equals("Comics", StringComparison.OrdinalIgnoreCase))
                    {
                        user.ComicAccess = dto.HasAccess;
                    }

                    _dbContext.MenuAccesses.Add(item);
                }

                await _userManager.UpdateAsync(user);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Menu permissions updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to save menu permissions", Error = ex.Message });
            }
        }

        // GET: api/admin/menu-access/my-permissions
        [HttpGet("my-permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                return await GetUserMenuPermissions(user.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to load your permissions", Error = ex.Message });
            }
        }
    }
}
