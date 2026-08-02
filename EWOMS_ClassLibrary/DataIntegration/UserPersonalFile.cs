using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWOMS_ClassLibrary.DataIntegration
{
    public class UserPersonalFile
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        public string? FileType { get; set; } // nullable - ContentType may be empty
        
        public long FileSize { get; set; }
        
        public DateTime UploadDate { get; set; } = DateTime.Now;
        
        [ForeignKey("UserId")]
        public virtual MasterUser User { get; set; }
    }
}
