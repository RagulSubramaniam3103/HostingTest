using System;

namespace EWOMS_ClassLibrary.DataIntegration
{
    public class UserNote
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string Color { get; set; } = "#fef08a"; // Default pastel yellow
        public bool IsFlagged { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
