using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Picture
    {
        [Key]
        public int PictureId { get; set; }

        public Property Property { get; set; }
        public int PropertyId { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(500)")]
        public string ImagePath { get; set; }
    }
}
