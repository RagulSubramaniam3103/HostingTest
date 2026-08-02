using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWOMS_ClassLibrary.DataIntegration
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public string SenderId { get; set; }
        public string? ReceiverId { get; set; }

        public int? GroupId { get; set; }

        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // ðŸŸ¡ DELIVERY STATUS
        public bool IsDelivered { get; set; } = false;
        public DateTime? DeliveredAt { get; set; }

        // ðŸ”µ READ STATUS
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // ðŸ–¼ï¸ IMAGE SUPPORT
        public string? Image { get; set; }

        // ðŸŽ¥ VIDEO SUPPORT
        public string? Video { get; set; }

        // ðŸ“„ DOCUMENT SUPPORT
        public string? Document { get; set; }
        public string? FileName { get; set; }
        
        // ðŸ—‘ï¸ SOFT DELETE SUPPORT
        public bool IsDeleted { get; set; } = false;
        public string? DeletedBy { get; set; }
    }
}
