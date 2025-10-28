#nullable enable
using API.Models.Enums;

namespace API.Imoveis.DTOs
{
    public class UpdatePropertyDTO
    {
        public int PropertyId { get; set; }
        public string Title { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public decimal Price { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public Topologies Topology { get; set; }
        public TypeOperations TypeOperation { get; set; }
        public decimal Area { get; set; }
        public short Floor { get; set; }
        public byte NumberOfFloors { get; set; }
        public byte Bedrooms { get; set; }
        public byte Bathrooms { get; set; }
        public bool HasGarage { get; set; }
        public bool HasElevator { get; set; }
        public short BuiltAt { get; set; }
        public bool AirConditioning { get; set; }
        public bool Furnished { get; set; }
        public bool Balcony { get; set; }
        public bool Terrace { get; set; }
    }
}
