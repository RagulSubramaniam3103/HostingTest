using EWOMS_ClassLibrary.DataControlled;
using EWOMS_ClassLibrary.DataIntegration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EWOMS_Application_CQRS.Commands.UserPost
{
    public class TogglePersonalFileLikeHandler
    {
        private readonly ApplicationDbContext _context;

        public TogglePersonalFileLikeHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(int fileId, string userId)
        {
            var existingLike = await _context.PersonalFileLikes
                .FirstOrDefaultAsync(l => l.FileId == fileId && l.UserId == userId);

            bool isLiked = false;
            if (existingLike == null)
            {
                var newLike = new PersonalFileLike
                {
                    FileId = fileId,
                    UserId = userId,
                    LikedAt = DateTime.UtcNow
                };
                await _context.PersonalFileLikes.AddAsync(newLike);
                isLiked = true;
            }
            else
            {
                _context.PersonalFileLikes.Remove(existingLike);
                isLiked = false;
            }

            await _context.SaveChangesAsync();

            // Get updated like count
            var likeCount = await _context.PersonalFileLikes.CountAsync(l => l.FileId == fileId);

            return new
            {
                status = isLiked ? "liked" : "unliked",
                isLiked = isLiked,
                likeCount = likeCount
            };
        }
    }
}
