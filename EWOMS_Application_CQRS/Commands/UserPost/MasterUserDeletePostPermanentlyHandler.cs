using EWOMS_ClassLibrary.DataControlled;
using EWOMS_ClassLibrary.DataIntegration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EWOMS_Application_CQRS.Commands.UserPost
{
    public class MasterUserDeletePostPermanentlyHandler
    {
        private readonly ApplicationDbContext _context;

        public MasterUserDeletePostPermanentlyHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(MasterUserDeletePostPermanentlyCommand command)
        {
            var deletedPost = await _context.DeleteUserPost.FirstOrDefaultAsync(p => p.SNo == command.SNo);
            if (deletedPost == null)
                return "Decommissioned post not found in archives.";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.DeleteUserPost.Remove(deletedPost);

                // Add Audit Log
                var auditLog = new AdminAuditLog
                {
                    AdminId = command.AdminId ?? "Admin",
                    AdminName = "Administrator",
                    Action = "Permanent Post Deletion",
                    TargetId = deletedPost.UId.ToString(),
                    TargetName = $"Post by {deletedPost.UserId}",
                    Details = $"Decommissioned post (Original ID: {deletedPost.UId}, Caption: '{deletedPost.Caption}') permanently deleted from secure archives.",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "N/A"
                };
                _context.AdminAuditLogs.Add(auditLog);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return "Post permanently purged from archives successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return $"Error: {ex.Message}";
            }
        }
    }
}
