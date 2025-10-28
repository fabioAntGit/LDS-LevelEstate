using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Agent : User
    {
        [Required]
        [Column(TypeName = "NVARCHAR(30)")]
        [StringLength(30)]
        public string AMICode { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(15)")]
        [StringLength(15)]
        public string ReferralCode { get; set; }

        public ICollection<Badge> Badges { get; set; }

        public ICollection<Property> Properties { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MonthlyPoints { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int AnnualPoints { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double AllMoneyEarned { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double MoneyEarnedLastMonth { get; set; }

    }
}
