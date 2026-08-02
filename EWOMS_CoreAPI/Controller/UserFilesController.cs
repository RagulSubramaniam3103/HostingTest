using EWOMS_ClassLibrary.DataControlled;
using EWOMS_ClassLibrary.DataIntegration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace EWOMS_CoreAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserFilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserFilesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [AllowAnonymous]
        [HttpGet("InitDb")]
        public IActionResult InitDb()
        {
            try
            {
                string sql = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EWO_UserPersonalFiles]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [EWO_UserPersonalFiles] (
                            [Id] int NOT NULL IDENTITY,
                            [UserId] nvarchar(450) NOT NULL,
                            [FileName] nvarchar(max) NOT NULL,
                            [FilePath] nvarchar(max) NOT NULL,
                            [FileType] nvarchar(max) NOT NULL,
                            [FileSize] bigint NOT NULL,
                            [UploadDate] datetime2 NOT NULL,
                            CONSTRAINT [PK_EWO_UserPersonalFiles] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_EWO_UserPersonalFiles_EWO_MasterUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [EWO_MasterUser] ([Id]) ON DELETE CASCADE
                        );
                        CREATE INDEX [IX_EWO_UserPersonalFiles_UserId] ON [EWO_UserPersonalFiles] ([UserId]);
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[EWO_MasterUser]') AND name = 'ComicAccess')
                        ALTER TABLE [dbo].[EWO_MasterUser] ADD [ComicAccess] bit NOT NULL DEFAULT 0;

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[EWO_MasterUser]') AND name = 'PrivateImageAccess')
                        ALTER TABLE [dbo].[EWO_MasterUser] ADD [PrivateImageAccess] bit NOT NULL DEFAULT 0;

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[EWO_ChatMessage]') AND name = 'DeletedBy')
                        ALTER TABLE [EWO_ChatMessage] ADD [DeletedBy] nvarchar(max) NULL;

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[EWO_ChatMessage]') AND name = 'IsDeleted')
                        ALTER TABLE [EWO_ChatMessage] ADD [IsDeleted] bit NOT NULL DEFAULT 0;

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[EWO_ChatGroup]') AND name = 'ProfileImage')
                        ALTER TABLE [EWO_ChatGroup] ADD [ProfileImage] nvarchar(max) NULL;

                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EWO_PersonalFileLike]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [EWO_PersonalFileLike] (
                            [Id] int NOT NULL IDENTITY,
                            [FileId] int NOT NULL,
                            [UserId] nvarchar(450) NOT NULL,
                            [LikedAt] datetime2 NOT NULL,
                            CONSTRAINT [PK_EWO_PersonalFileLike] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_EWO_PersonalFileLike_EWO_UserPersonalFiles_FileId] FOREIGN KEY ([FileId]) REFERENCES [EWO_UserPersonalFiles] ([Id]) ON DELETE CASCADE,
                            CONSTRAINT [FK_EWO_PersonalFileLike_EWO_MasterUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [EWO_MasterUser] ([Id]) ON DELETE NO ACTION
                        );
                        CREATE INDEX [IX_EWO_PersonalFileLike_FileId] ON [EWO_PersonalFileLike] ([FileId]);
                        CREATE INDEX [IX_EWO_PersonalFileLike_UserId] ON [EWO_PersonalFileLike] ([UserId]);
                    END

                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EWO_PartnerConnection]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [EWO_PartnerConnection] (
                            [Id] int NOT NULL IDENTITY,
                            [SenderId] nvarchar(450) NOT NULL,
                            [ReceiverId] nvarchar(450) NOT NULL,
                            [IsAccepted] bit NOT NULL,
                            [RequestDate] datetime2 NOT NULL,
                            [ConnectedAt] datetime2 NULL,
                            CONSTRAINT [PK_EWO_PartnerConnection] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_EWO_PartnerConnection_EWO_MasterUser_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [EWO_MasterUser] ([Id]) ON DELETE CASCADE,
                            CONSTRAINT [FK_EWO_PartnerConnection_EWO_MasterUser_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [EWO_MasterUser] ([Id]) ON DELETE NO ACTION
                        );
                        CREATE INDEX [IX_EWO_PartnerConnection_SenderId] ON [EWO_PartnerConnection] ([SenderId]);
                        CREATE INDEX [IX_EWO_PartnerConnection_ReceiverId] ON [EWO_PartnerConnection] ([ReceiverId]);
                    END

                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[EWO_PartnerSharedPhoto]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [EWO_PartnerSharedPhoto] (
                            [Id] int NOT NULL IDENTITY,
                            [ConnectionId] int NOT NULL,
                            [UploaderId] nvarchar(450) NOT NULL,
                            [FileName] nvarchar(max) NOT NULL,
                            [FilePath] nvarchar(max) NOT NULL,
                            [FileType] nvarchar(max) NOT NULL,
                            [FileSize] bigint NOT NULL,
                            [SharedAt] datetime2 NOT NULL,
                            CONSTRAINT [PK_EWO_PartnerSharedPhoto] PRIMARY KEY ([Id])
                        );
                        CREATE INDEX [IX_EWO_PartnerSharedPhoto_ConnectionId] ON [EWO_PartnerSharedPhoto] ([ConnectionId]);
                        CREATE INDEX [IX_EWO_PartnerSharedPhoto_UploaderId] ON [EWO_PartnerSharedPhoto] ([UploaderId]);
                    END
                ";

                _context.Database.ExecuteSqlRaw(sql);
                return Ok("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Message = "Identity context not found" });

            // Ensure WebRootPath exists (wwwroot)
            if (string.IsNullOrEmpty(_env.WebRootPath))
            {
                // Fallback for some environments where WebRootPath might be null
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }

            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // Create directory: wwwroot/UserFiles/{UserId}
            var userFolder = Path.Combine(rootPath, "UserFiles", userId);
            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            // Generate unique filename to avoid collisions
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(userFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save metadata to DB
            var personalFile = new UserPersonalFile
            {
                UserId = userId,
                FileName = file.FileName,
                FilePath = $"/UserFiles/{userId}/{uniqueFileName}",
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadDate = DateTime.Now
            };

            _context.UserPersonalFiles.Add(personalFile);
            await _context.SaveChangesAsync();

            return Ok(personalFile);
        }

        [HttpGet("MyFiles")]
        public async Task<IActionResult> GetMyFiles()
        {
            try 
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Message = "Identity context not found" });

                var files = await _context.UserPersonalFiles
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.UploadDate)
                    .Select(f => new {
                        f.Id,
                        f.UserId,
                        f.FileName,
                        f.FilePath,
                        f.FileType,
                        f.FileSize,
                        f.UploadDate,
                        LikeCount = _context.PersonalFileLikes.Count(l => l.FileId == f.Id),
                        IsLiked = _context.PersonalFileLikes.Any(l => l.FileId == f.Id && l.UserId == userId)
                    })
                    .ToListAsync();

                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Failed to retrieve assets", Error = ex.Message });
            }
        }

        public class SiteLoginImageDto
        {
            public string FilePath { get; set; }
        }

        [AllowAnonymous]
        [HttpGet("SiteLoginImage")]
        public IActionResult GetSiteLoginImage()
        {
            try
            {
                var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var configPath = Path.Combine(rootPath, "site-login-image.json");
                if (!System.IO.File.Exists(configPath))
                {
                    return NotFound(new { Message = "No site login image configured." });
                }

                var json = System.IO.File.ReadAllText(configPath);
                var dto = JsonSerializer.Deserialize<SiteLoginImageDto>(json);
                if (dto == null || string.IsNullOrEmpty(dto.FilePath))
                {
                    return NotFound(new { Message = "No valid site login image configured." });
                }

                var url = $"{Request.Scheme}://{Request.Host}{dto.FilePath}";
                return Ok(new { filePath = dto.FilePath, url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Failed to load site login image", Error = ex.Message });
            }
        }

        [HttpPost("SetSiteLoginImage")]
        public async Task<IActionResult> SetSiteLoginImage([FromBody] SiteLoginImageDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.FilePath))
                return BadRequest(new { Message = "FilePath is required." });

            try
            {
                var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fullPath = Path.Combine(rootPath, dto.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(new { Message = "Target image not found on disk." });
                }

                var configPath = Path.Combine(rootPath, "site-login-image.json");
                var json = JsonSerializer.Serialize(dto);
                await System.IO.File.WriteAllTextAsync(configPath, json);

                return Ok(new { Message = "Site login image updated.", filePath = dto.FilePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Failed to persist site login image", Error = ex.Message });
            }
        }

        [HttpGet("UserFiles/{targetUserId}")]
        public async Task<IActionResult> GetUserFiles(string targetUserId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

                // Enforce connection security check: must be self OR connected partner (IsAccepted status in PartnerConnections)
                if (currentUserId != targetUserId)
                {
                    var isConnected = await _context.PartnerConnections
                        .AnyAsync(pc => 
                            ((pc.SenderId == currentUserId && pc.ReceiverId == targetUserId) || 
                             (pc.SenderId == targetUserId && pc.ReceiverId == currentUserId)) 
                            && pc.IsAccepted);

                    if (!isConnected)
                    {
                        return Forbid();
                    }
                }

                var files = await _context.UserPersonalFiles
                    .Where(f => f.UserId == targetUserId)
                    .OrderByDescending(f => f.UploadDate)
                    .Select(f => new {
                        f.Id,
                        f.UserId,
                        f.FileName,
                        f.FilePath,
                        f.FileType,
                        f.FileSize,
                        f.UploadDate,
                        LikeCount = _context.PersonalFileLikes.Count(l => l.FileId == f.Id),
                        IsLiked = _context.PersonalFileLikes.Any(l => l.FileId == f.Id && l.UserId == currentUserId)
                    })
                    .ToListAsync();

                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Failed to retrieve user assets", Error = ex.Message });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            var file = await _context.UserPersonalFiles.FindAsync(id);

            if (file == null) return NotFound();
            if (file.UserId != userId) return Forbid();

            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            
            // Delete physical file
            var physicalPath = Path.Combine(rootPath, file.FilePath.TrimStart('/').Replace("/", "\\"));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            _context.UserPersonalFiles.Remove(file);
            await _context.SaveChangesAsync();

            return Ok(new { message = "File deleted successfully" });
        }

        public class PartnerRequestDto
        {
            public string ReceiverId { get; set; }
        }

        [HttpPost("SendPartnerRequest")]
        public async Task<IActionResult> SendPartnerRequest([FromBody] PartnerRequestDto dto)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(senderId)) return Unauthorized();

            if (senderId == dto.ReceiverId)
                return BadRequest("You cannot send a partner request to yourself");

            // check receiver user
            var receiverExists = await _context.Users.AnyAsync(x => x.Id == dto.ReceiverId);
            if (!receiverExists)
                return NotFound("User not found");

            // already exists check
            var exists = await _context.PartnerConnections.AnyAsync(x =>
                (x.SenderId == senderId && x.ReceiverId == dto.ReceiverId) ||
                (x.SenderId == dto.ReceiverId && x.ReceiverId == senderId));

            if (exists)
                return Ok(new { message = "Request already exists or partner already connected" });

            var request = new PartnerConnection
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                IsAccepted = false,
                RequestDate = DateTime.Now
            };

            _context.PartnerConnections.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Partner request sent successfully" });
        }

        [HttpPost("AcceptPartnerRequest/{requestId}")]
        public async Task<IActionResult> AcceptPartnerRequest(int requestId)
        {
            if (requestId <= 0) return BadRequest("Invalid request ID");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var request = await _context.PartnerConnections.FirstOrDefaultAsync(x => x.Id == requestId);
            if (request == null)
                return NotFound("Request not found");

            // Security Check: Only the receiver can accept the request
            if (request.ReceiverId != userId)
                return Unauthorized("You are not authorized to accept this request");

            request.IsAccepted = true;
            request.ConnectedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Partner request accepted", requestId = requestId });
        }

        [HttpPost("DeclinePartnerRequest/{requestId}")]
        public async Task<IActionResult> DeclinePartnerRequest(int requestId)
        {
            if (requestId <= 0) return BadRequest("Invalid request ID");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var request = await _context.PartnerConnections.FirstOrDefaultAsync(x => x.Id == requestId);
            if (request == null)
                return NotFound("Request not found");

            // Security Check: Sender or Receiver can decline or remove partner
            if (request.ReceiverId != userId && request.SenderId != userId)
                return Unauthorized("You are not authorized to manage this connection");

            _context.PartnerConnections.Remove(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Partner connection declined/removed", requestId = requestId });
        }

        [HttpGet("GetPartnerRequests")]
        public async Task<IActionResult> GetPartnerRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var requests = await _context.PartnerConnections
                .Where(x => x.ReceiverId == userId && !x.IsAccepted)
                .Select(x => new
                {
                    x.Id,
                    x.SenderId,
                    x.ReceiverId,
                    x.RequestDate,
                    x.IsAccepted,
                    Sender = _context.Users
                        .Where(u => u.Id == x.SenderId)
                        .Select(u => new
                        {
                            u.Id,
                            u.UserName,
                            u.FullName,
                            u.ProfileImage
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("GetPartners")]
        public async Task<IActionResult> GetPartners()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var partners = await _context.PartnerConnections
                .Where(x => (x.SenderId == userId || x.ReceiverId == userId) && x.IsAccepted)
                .Select(x => new
                {
                    ConnectionId = x.Id,
                    PartnerId = x.SenderId == userId ? x.ReceiverId : x.SenderId,
                    ConnectedAt = x.ConnectedAt
                })
                .ToListAsync();

            // Join with user info and include ConnectionId
            var result = await Task.WhenAll(partners.Select(async p =>
            {
                var user = await _context.Users
                    .Where(u => u.Id == p.PartnerId)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.FullName,
                        u.ProfileImage
                    })
                    .FirstOrDefaultAsync();

                return new
                {
                    ConnectionId = p.ConnectionId,
                    UserId = user != null ? user.Id : p.PartnerId,
                    UserName = user != null ? (user.UserName ?? "Unknown") : "Unknown",
                    FullName = user != null ? (user.FullName ?? "Unknown") : "Unknown",
                    ProfileImage = user != null ? user.ProfileImage : null,
                    ConnectedAt = p.ConnectedAt
                };
            }));

            return Ok(result);
        }

        [HttpGet("GetPartnerConnections")]
        public async Task<IActionResult> GetPartnerConnections()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var connections = await _context.PartnerConnections
                .Where(x => (x.SenderId == userId || x.ReceiverId == userId) && x.IsAccepted)
                .Select(x => new
                {
                    x.Id,
                    x.SenderId,
                    x.ReceiverId,
                    x.IsAccepted,
                    x.ConnectedAt
                })
                .ToListAsync();

            return Ok(connections);
        }


        /// <summary>
        /// Upload a photo specifically for a partner connection vault.
        /// Only the sender or receiver of the accepted connection can upload.
        /// </summary>
        [HttpPost("UploadPartnerPhoto/{connectionId}")]
        public async Task<IActionResult> UploadPartnerPhoto(int connectionId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Verify connection exists, is accepted, and the requester is one of the partners
            var connection = await _context.PartnerConnections
                .FirstOrDefaultAsync(x => x.Id == connectionId && x.IsAccepted &&
                                          (x.SenderId == userId || x.ReceiverId == userId));
            if (connection == null)
                return Forbid();

            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(rootPath, "PartnerPhotos", connectionId.ToString());
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(folder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var photo = new PartnerSharedPhoto
            {
                ConnectionId = connectionId,
                UploaderId = userId,
                FileName = file.FileName,
                FilePath = $"/PartnerPhotos/{connectionId}/{uniqueFileName}",
                FileType = file.ContentType,
                FileSize = file.Length,
                SharedAt = DateTime.Now
            };

            _context.PartnerSharedPhotos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(photo);
        }

        /// <summary>
        /// Get all shared photos for a specific partner connection.
        /// Only accessible by the two partners in that connection.
        /// </summary>
        [HttpGet("GetPartnerSharedPhotos/{connectionId}")]
        public async Task<IActionResult> GetPartnerSharedPhotos(int connectionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Verify the requesting user is part of this accepted connection
            var connection = await _context.PartnerConnections
                .FirstOrDefaultAsync(x => x.Id == connectionId && x.IsAccepted &&
                                          (x.SenderId == userId || x.ReceiverId == userId));
            if (connection == null)
                return Forbid();

            var photos = await _context.PartnerSharedPhotos
                .Where(p => p.ConnectionId == connectionId)
                .OrderByDescending(p => p.SharedAt)
                .Select(p => new
                {
                    p.Id,
                    p.ConnectionId,
                    p.UploaderId,
                    p.FileName,
                    p.FilePath,
                    p.FileType,
                    p.FileSize,
                    p.SharedAt,
                    UploaderName = _context.Users
                        .Where(u => u.Id == p.UploaderId)
                        .Select(u => u.FullName ?? u.UserName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(photos);
        }

        /// <summary>
        /// Delete a shared photo from a partner vault (only uploader can delete).
        /// </summary>
        [HttpDelete("DeletePartnerPhoto/{photoId}")]
        public async Task<IActionResult> DeletePartnerPhoto(int photoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var photo = await _context.PartnerSharedPhotos.FindAsync(photoId);
            if (photo == null) return NotFound();
            if (photo.UploaderId != userId) return Forbid();

            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var physicalPath = Path.Combine(rootPath, photo.FilePath.TrimStart('/').Replace("/", "\\"));
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            _context.PartnerSharedPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Photo deleted" });
        }
    }
}
