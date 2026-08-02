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
    [Route("api/user/notes")]
    [ApiController]
    [Authorize]
    public class UserNotesController : ControllerBase
    {
        private readonly UserManager<MasterUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public UserNotesController(UserManager<MasterUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: api/user/notes
        [HttpGet]
        public async Task<IActionResult> GetNotes([FromQuery] string? search)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                var query = _dbContext.UserNotes.Where(n => n.UserId == user.Id);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var cleanSearch = search.Trim().ToLower();
                    query = query.Where(n => (n.Title != null && n.Title.ToLower().Contains(cleanSearch)) || 
                                             (n.Content != null && n.Content.ToLower().Contains(cleanSearch)));
                }

                var notes = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to fetch notes", Error = ex.Message });
            }
        }

        public class NoteCreateRequest
        {
            public string? Title { get; set; }
            public string? Content { get; set; }
            public string? Color { get; set; }
        }

        // POST: api/user/notes
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] NoteCreateRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                var note = new UserNote
                {
                    UserId = user.Id,
                    Title = request.Title,
                    Content = request.Content,
                    Color = string.IsNullOrWhiteSpace(request.Color) ? "#fef08a" : request.Color,
                    IsFlagged = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.UserNotes.Add(note);
                await _dbContext.SaveChangesAsync();

                return Ok(note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to create note", Error = ex.Message });
            }
        }

        public class NoteUpdateRequest
        {
            public string? Title { get; set; }
            public string? Content { get; set; }
            public string? Color { get; set; }
        }

        // PUT: api/user/notes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] NoteUpdateRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                var note = await _dbContext.UserNotes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);
                if (note == null)
                {
                    return NotFound("Note not found or you do not have permission to edit it.");
                }

                note.Title = request.Title;
                note.Content = request.Content;
                if (!string.IsNullOrWhiteSpace(request.Color))
                {
                    note.Color = request.Color;
                }
                note.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return Ok(note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to update note", Error = ex.Message });
            }
        }

        // DELETE: api/user/notes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                var note = await _dbContext.UserNotes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);
                if (note == null)
                {
                    return NotFound("Note not found or you do not have permission to delete it.");
                }

                _dbContext.UserNotes.Remove(note);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Note deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to delete note", Error = ex.Message });
            }
        }

        // POST: api/user/notes/{id}/toggle-flag
        [HttpPost("{id}/toggle-flag")]
        public async Task<IActionResult> ToggleFlag(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                var note = await _dbContext.UserNotes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);
                if (note == null)
                {
                    return NotFound("Note not found or you do not have permission to update it.");
                }

                note.IsFlagged = !note.IsFlagged;
                note.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return Ok(note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to toggle flag state", Error = ex.Message });
            }
        }
    }
}
