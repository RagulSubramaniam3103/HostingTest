using EWOMS_ClassLibrary.DataControlled;
using EWOMS_ClassLibrary.DataIntegration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EWOMS_Application_CQRS.Commands.UserPost
{
    public class TogglePostLikeHandler
    {
        private readonly ApplicationDbContext _context;

        public TogglePostLikeHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(int postId, string userId)
        {
            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            bool isLiked = false;
            if (existingLike == null)
            {
                var newLike = new PostLike
                {
                    PostId = postId,
                    UserId = userId,
                    LikedAt = DateTime.UtcNow
                };
                await _context.PostLikes.AddAsync(newLike);
                isLiked = true;
            }
            else
            {
                _context.PostLikes.Remove(existingLike);
                isLiked = false;
            }

            await _context.SaveChangesAsync();

            // Get updated like count
            var likeCount = await _context.PostLikes.CountAsync(l => l.PostId == postId);

            return new
            {
                status = isLiked ? "liked" : "unliked",
                isLiked = isLiked,
                likeCount = likeCount
            };
        }
    }
}
