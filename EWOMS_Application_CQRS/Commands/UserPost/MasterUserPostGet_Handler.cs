using EWOMS_ClassLibrary.DataControlled;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWOMS_Application_CQRS.Commands.UserPost
{
    public class MasterUserPostGet_Handler
    {
        private readonly ApplicationDbContext _context;

        public MasterUserPostGet_Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> Handle(string? userId, string? currentUserId = null)
        {
            try {
                // 1. Get the list of people the current user is following
                var followingIds = new List<string>();
                bool isAdmin = false;

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    followingIds = await _context.EWOMS_Followers
                        .Where(f => f.FollowerId == currentUserId)
                        .Select(f => f.FollowingId)
                        .ToListAsync();

                    // Check if current user is admin
                    var user = await _context.Users.FindAsync(currentUserId);
                    if (user != null)
                    {
                        // Assuming you have a way to check roles, or just check if they are in Master_Admins table
                        isAdmin = await _context.Master_Admins.AnyAsync(a => a.UserId == currentUserId);
                    }
                }

                var query = _context.UserPost
                    .Where(p => !p.IsDeleted && p.IsActive)
                    .AsQueryable();

                // 2. Filter by Target User (if viewing a specific profile)
                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(p => p.UserId == userId);
                }

                var postsQuery = from p in query
                                 join u in _context.Users on p.UserId equals u.Id
                                 // Visibility check:
                                 // 1. Admin/Manager see all
                                 // 2. Owner sees own
                                 // 3. Anyone sees public posts (!IsPrivate)
                                 // 4. Followers see private posts
                                 where isAdmin || 
                                       u.Id == currentUserId || 
                                       !u.IsPrivate || 
                                       (followingIds != null && followingIds.Contains(u.Id))
                                 select new
                                 {
                                     p.Id,
                                     p.UserId,
                                     UserName = u.UserName,
                                     UserImage = u.ProfileImage != null ? Convert.ToBase64String(u.ProfileImage) : null,
                                     p.Caption,
                                     PostImage = p.profileimage != null ? Convert.ToBase64String(p.profileimage) : null,
                                     p.VideoUrl,
                                     p.IsBlurred,
                                     p.CreatedAt,
                                     UserRole = "user", // Simplified for now to rule out errors
                                     IsPrivate = u.IsPrivate,
                                     LikeCount = _context.PostLikes.Count(l => l.PostId == p.Id),
                                     IsLiked = !string.IsNullOrEmpty(currentUserId) && _context.PostLikes.Any(l => l.PostId == p.Id && l.UserId == currentUserId),
                                     IsSaved = !string.IsNullOrEmpty(currentUserId) && _context.SavedPosts.Any(s => s.PostId == p.Id && s.UserId == currentUserId)
                                 };

                var posts = await postsQuery
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return posts.Cast<object>().ToList();
            } catch (Exception ex) {
                System.IO.File.WriteAllText("backend_error_log.txt", ex.ToString());
                throw;
            }
        }
    }
}
