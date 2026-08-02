using EWOMS_ClassLibrary.DataControlled;
using EWOMS_ClassLibrary.DataIntegration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EWOMS_CoreAPI.Controller
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<MasterUser> _userManager;

        public AdminDashboardController(ApplicationDbContext context, UserManager<MasterUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. Summary API
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsersToday = await _context.Users.CountAsync(u => u.IsActive); // Placeholder for login check
            var totalPosts = await _context.UserPost.CountAsync(p => !p.IsDeleted);
            var pendingPosts = await _context.UserPost.CountAsync(p => !p.IsActive && !p.IsDeleted);
            var flaggedPosts = await _context.UserPost.CountAsync(p => p.IsBlurred && !p.IsDeleted);

            return Ok(new
            {
                TotalUsers = totalUsers,
                ActiveUsersToday = activeUsersToday,
                TotalPosts = totalPosts,
                PendingPosts = pendingPosts,
                FlaggedPosts = flaggedPosts
            });
        }

        // 2. Activity Trends API
        [HttpGet("trends")]
        public async Task<IActionResult> GetTrends()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var registrations = await _context.Users
                .Where(u => u.CreatedUser != null && u.CreatedUser >= thirtyDaysAgo)
                .GroupBy(u => u.CreatedUser!.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToListAsync();

            var posts = await _context.UserPost
                .Where(p => p.CreatedAt >= thirtyDaysAgo && !p.IsDeleted)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToListAsync();

            return Ok(new
            {
                UserRegistrations = registrations,
                PostCreation = posts
            });
        }

        // 3. Moderation Queue API
        [HttpGet("moderation-queue")]
        public async Task<IActionResult> GetModerationQueue()
        {
            var pendingPosts = await _context.UserPost
                .Where(p => (!p.IsActive || p.IsBlurred) && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .Select(p => new
                {
                    p.Id,
                    Caption = p.Caption ?? string.Empty,
                    p.CreatedAt,
                    p.IsActive,
                    p.IsBlurred,
                    p.UserId,
                    UserName = _context.Users.Where(u => u.Id == p.UserId).Select(u => u.FullName).FirstOrDefault() ?? "Unknown",
                    Status = p.IsBlurred ? "Flagged" : "Pending Approval",
                    postImage = p.profileimage != null ? Convert.ToBase64String(p.profileimage) : null
                })
                .ToListAsync();

            return Ok(pendingPosts);
        }

        [HttpPost("approve-post")]
        public async Task<IActionResult> ApprovePost([FromQuery] int postId)
        {
            var post = await _context.UserPost.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return NotFound("Broadcast asset not found.");

            post.IsActive = true;
            post.IsBlurred = false;
            post.ModeratedBy = User.Identity?.Name ?? "Admin";
            post.ModeratedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Broadcast asset successfully approved and restored to feed." });
        }

        // 4. User Insights API
        [HttpGet("user-insights")]
        public async Task<IActionResult> GetUserInsights()
        {
            var mostActiveUsers = await _context.UserPost
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    PostCount = g.Count(),
                    FullName = _context.Users.Where(u => u.Id == g.Key).Select(u => u.FullName).FirstOrDefault() ?? "Unknown"
                })
                .OrderByDescending(x => x.PostCount)
                .Take(10)
                .ToListAsync();

            var inactiveUsers = await _context.Users
                .Where(u => !_context.UserPost.Any(p => p.UserId == u.Id && !p.IsDeleted))
                .Take(10)
                .Select(u => new
                {
                    u.Id,
                    FullName = u.FullName ?? "Unknown",
                    Email = u.Email ?? "No Email",
                    u.CreatedUser
                })
                .ToListAsync();

            return Ok(new
            {
                MostActiveUsers = mostActiveUsers,
                InactiveUsers = inactiveUsers
            });
        }
    }
}
