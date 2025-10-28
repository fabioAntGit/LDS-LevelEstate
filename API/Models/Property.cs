using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Models.Enums;

namespace API.Models
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }

        public Agent Agent { get; set; }
        public int AgentId { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(200)")]
        [StringLength(200)]
        public string Street { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(10)")]
        [StringLength(10)]
        public string PostalCode { get; set; }

        [Required]
        [Column(TypeName = "decimal(9,6)")]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(9,6)")]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(100)")]
        [StringLength(100)]
        public string Title { get; set; }

        [Column(TypeName = "NVARCHAR(1000)")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public Status Status { get; set; } = Status.Available;

        [Required]
        public Topologies Topology { get; set; }

        [Required]
        public TypeOperations TypeOperation { get; set; }

        [MinLength(1)]
        public ICollection<Picture> Pictures { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Area { get; set; }

        [Required]
        [Column(TypeName = "SMALLINT")]
        public short Floor { get; set; }

        [Required]
        [Column(TypeName = "TINYINT")]
        public byte NumberOfFloors { get; set; }

        [Required]
        [Column(TypeName = "TINYINT")]
        public byte Bedrooms { get; set; }

        [Required]
        [Column(TypeName = "TINYINT")]
        public byte Bathrooms { get; set; }


        public bool HasGarage { get; set; }

        public bool HasElevator { get; set; }

        [Required]
        [Column(TypeName = "SMALLINT")]
        [Range(1800, 2100)]
        public short BuiltAt { get; set; }

        public bool AirConditioning { get; set; }

        public bool Furnished { get; set; }

        public bool Balcony { get; set; }

        public bool Terrace { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
