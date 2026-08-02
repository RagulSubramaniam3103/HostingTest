using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWOMS_ClassLibrary.DataIntegration
{
    /// <summary>
    /// Represents a photo shared specifically within a partner connection vault.
    /// Only visible to the two connected partners (sender + receiver in EWO_PartnerConnection).
    /// </summary>
    public class PartnerSharedPhoto
    {
        [Key]
        public int Id { get; set; }

        /// <summary>The partner connection this photo is shared within.</summary>
        [Required]
        public int ConnectionId { get; set; }

        /// <summary>The user who uploaded / shared this photo.</summary>
        [Required]
        public string UploaderId { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }

        public DateTime SharedAt { get; set; } = DateTime.Now;
    }
}
