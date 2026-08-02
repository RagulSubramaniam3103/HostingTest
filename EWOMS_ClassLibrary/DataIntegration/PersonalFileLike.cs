using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWOMS_ClassLibrary.DataIntegration
{
    public class PersonalFileLike
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FileId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("FileId")]
        public virtual UserPersonalFile PersonalFile { get; set; }

        [ForeignKey("UserId")]
        public virtual MasterUser User { get; set; }
    }
}
