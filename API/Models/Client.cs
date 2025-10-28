using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Client : User
    {
        public ICollection<Property> Favorite { get; set; }

        [Column(TypeName = "NVARCHAR(15)")]
        [StringLength(15)]
        public string? ReferralCodeUsed { get; set; }
    }
}
