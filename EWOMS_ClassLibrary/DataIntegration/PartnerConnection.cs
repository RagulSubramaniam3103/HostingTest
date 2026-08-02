using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWOMS_ClassLibrary.DataIntegration
{
    public class PartnerConnection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        public bool IsAccepted { get; set; } = false;

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public DateTime? ConnectedAt { get; set; }
    }
}
