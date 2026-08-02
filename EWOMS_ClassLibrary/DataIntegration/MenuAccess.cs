using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EWOMS_ClassLibrary.DataIntegration
{
    [Table("EWO_MenuAccess")]
    public class MenuAccess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string MenuName { get; set; } = string.Empty;

        public bool HasAccess { get; set; } = true;

        public bool IsPrivate { get; set; } = false;
    }
}
