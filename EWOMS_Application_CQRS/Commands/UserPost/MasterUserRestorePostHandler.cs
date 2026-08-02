using EWOMS_ClassLibrary.DataControlled;
using EWOMS_ClassLibrary.DataIntegration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EWOMS_Application_CQRS.Commands.UserPost
{
    public class MasterUserRestorePostHandler
    {
        private readonly ApplicationDbContext _context;

        public MasterUserRestorePostHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(MasterUserRestorePostCommand command)
        {
            var deletedPost = await _context.DeleteUserPost.FirstOrDefaultAsync(p => p.SNo == command.SNo);
            if (deletedPost == null)
                return "Decommissioned post not found in archives.";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create active UserPost entity
                var restoredPost = new EWOMS_ClassLibrary.DataIntegration.UserPost
                {
                    UserId = deletedPost.UserId,
                    profileimage = deletedPost.ProfileImage,
                    Caption = deletedPost.Caption,
                    CreatedAt = deletedPost.CreatedAt,
                    IsActive = true,
                    IsBlurred = false,
                    IsDeleted = false,
                    ModeratedBy = command.AdminId,
                    ModeratedAt = DateTime.UtcNow
                };

                _context.UserPost.Add(restoredPost);
                _context.DeleteUserPost.Remove(deletedPost);

                // Add Audit Log
                var auditLog = new AdminAuditLog
                {
                    AdminId = command.AdminId ?? "Admin",
                    AdminName = "Administrator",
                    Action = "Post Restoration",
                    TargetId = deletedPost.UId.ToString(),
                    TargetName = $"Post by {deletedPost.UserId}",
                    Details = $"Decommissioned post (Original ID: {deletedPost.UId}) successfully restored to active intelligence feeds.",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "N/A"
                };
                _context.AdminAuditLogs.Add(auditLog);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return "Post successfully restored to active feeds.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return $"Error: {ex.Message}";
            }
        }
    }
}
