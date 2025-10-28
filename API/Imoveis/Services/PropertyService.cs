using API.Data;
using API.FavoritosNotificacoes.Notificacoes.Services;
using API.FavoritosNotificacoes.Notificacoes.Interfaces;
using API.Imoveis.DTOs;
using API.Imoveis.Exceptions;
using API.Imoveis.Interfaces;
using API.Models;
using API.Models.Enums;
using API.Perfis.Agente.Exceptions;
using API.Perfis.Agente.Interfaces;
using API.PesquisasFiltros.Pesquisas.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace API.Imoveis.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly LevelEstateContext context;
        private readonly INotificationService notificationService;
        private readonly IAgentService agentService;
        public PropertyService(LevelEstateContext context, INotificationService notificationService, IAgentService agentService)
        {
            this.context = context;
            this.notificationService = notificationService;
            this.agentService = agentService;
        }

        public PropertyDTO CreateProperty(CreatePropertyDTO dto, int userId)
        {
            if (dto == null)
                throw new ArgumentNullException("Property data cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.Street) ||
                string.IsNullOrWhiteSpace(dto.PostalCode))
                throw new InvalidPropertyDataException("Missing required property fields.");

            if (dto.Title.Length > 100)
                throw new InvalidPropertyDataException("Title cannot exceed 100 characters.");

            if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 1000)
                throw new InvalidPropertyDataException("Description cannot exceed 1000 characters.");

            var postalCodePattern = @"^\d{4}-\d{3}$";
            if (!Regex.IsMatch(dto.PostalCode, postalCodePattern))
                throw new InvalidPropertyDataException("Invalid postal code format.");

            if (dto.Price <= 0)
                throw new InvalidPropertyDataException("Price must be greater than zero.");

            if (dto.Area <= 0)
                throw new InvalidPropertyDataException("Area must be greater than zero.");

            if (dto.Bedrooms < 0)
                throw new InvalidPropertyDataException("Number of bedrooms cannot be negative.");

            if (dto.Bathrooms < 0)
                throw new InvalidPropertyDataException("Number of bathrooms cannot be negative.");

            if (dto.Latitude < -90 || dto.Latitude > 90)
                throw new InvalidPropertyDataException("Latitude must be between -90 and 90 degrees.");

            if (dto.Longitude < -180 || dto.Longitude > 180)
                throw new InvalidPropertyDataException("Longitude must be between -180 and 180 degrees.");

            var currentYear = DateTime.UtcNow.Year;
            if (dto.BuiltAt < 1900 || dto.BuiltAt > currentYear)
                throw new InvalidPropertyDataException("Invalid construction year.");

            if (!Enum.IsDefined(typeof(Topologies), dto.Topology))
                throw new InvalidPropertyDataException("Invalid property topology.");

            if (!Enum.IsDefined(typeof(TypeOperations), dto.TypeOperation))
                throw new InvalidPropertyDataException("Invalid property operation type.");

            dto.Title = CapitalizeWords(dto.Title);
            dto.Street = CapitalizeWords(dto.Street);
            dto.PostalCode = dto.PostalCode.Trim().ToUpper();
            dto.Description = dto.Description?.Trim();

            var property = new Property
            {
                Title = dto.Title,
                Street = dto.Street,
                PostalCode = dto.PostalCode,
                Description = dto.Description,
                Price = dto.Price,
                AgentId = userId,
                Topology = dto.Topology,
                TypeOperation = dto.TypeOperation,
                Area = dto.Area,
                Floor = dto.Floor,
                NumberOfFloors = dto.NumberOfFloors,
                Bedrooms = dto.Bedrooms,
                Bathrooms = dto.Bathrooms,
                HasGarage = dto.HasGarage,
                HasElevator = dto.HasElevator,
                BuiltAt = dto.BuiltAt,
                AirConditioning = dto.AirConditioning,
                Furnished = dto.Furnished,
                Balcony = dto.Balcony,
                Terrace = dto.Terrace,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Status = Status.Available,
                CreatedAt = DateTime.UtcNow
            };

            context.Properties.Add(property);
            context.SaveChanges();

            return new PropertyDTO
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                Price = property.Price,
                Status = property.Status,
                Street = property.Street,
                PostalCode = property.PostalCode,
                Area = property.Area,
                Topology = property.Topology,
                TypeOperation = property.TypeOperation,
                AgentId = property.AgentId,
                CreatedAt = property.CreatedAt
            };
        }

        public async Task<PropertyDTO> UpdatePropertyAsync(UpdatePropertyDTO dto, int userId)
        {
            if (dto == null)
                throw new ArgumentNullException("Property data cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.Street) ||
                string.IsNullOrWhiteSpace(dto.PostalCode))
                throw new InvalidPropertyDataException("Missing required property fields.");

            if (dto.Title.Length > 100)
                throw new InvalidPropertyDataException("Title cannot exceed 100 characters.");

            if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 1000)
                throw new InvalidPropertyDataException("Description cannot exceed 1000 characters.");

            var postalCodePattern = @"^\d{4}-\d{3}$";
            if (!Regex.IsMatch(dto.PostalCode, postalCodePattern))
                throw new InvalidPropertyDataException("Invalid postal code format.");

            if (dto.Price <= 0)
                throw new InvalidPropertyDataException("Price must be greater than zero.");

            if (dto.Area <= 0)
                throw new InvalidPropertyDataException("Area must be greater than zero.");

            if (dto.Bedrooms < 0)
                throw new InvalidPropertyDataException("Number of bedrooms cannot be negative.");

            if (dto.Bathrooms < 0)
                throw new InvalidPropertyDataException("Number of bathrooms cannot be negative.");

            if (dto.Latitude < -90 || dto.Latitude > 90)
                throw new InvalidPropertyDataException("Latitude must be between -90 and 90 degrees.");

            if (dto.Longitude < -180 || dto.Longitude > 180)
                throw new InvalidPropertyDataException("Longitude must be between -180 and 180 degrees.");

            var currentYear = DateTime.UtcNow.Year;
            if (dto.BuiltAt < 1900 || dto.BuiltAt > currentYear)
                throw new InvalidPropertyDataException("Invalid construction year.");

            if (!Enum.IsDefined(typeof(Topologies), dto.Topology))
                throw new InvalidPropertyDataException("Invalid property topology.");

            if (!Enum.IsDefined(typeof(TypeOperations), dto.TypeOperation))
                throw new InvalidPropertyDataException("Invalid property operation type.");

            var existing = context.Properties.FirstOrDefault(p => p.PropertyId == dto.PropertyId);
            if (existing == null)
                throw new PropertyNotFoundException("Property was not found.");

            if (existing.AgentId != userId)
                throw new AgentNotAuthorizedException("You are not allowed to update this property.");

            var oldPrice = existing.Price;
            var oldStatus = existing.Status;

            existing.Title = CapitalizeWords(dto.Title);
            existing.Street = CapitalizeWords(dto.Street);
            existing.PostalCode = dto.PostalCode.Trim().ToUpper();
            existing.Description = dto.Description?.Trim();
            existing.Price = dto.Price;
            existing.Status = dto.Status;
            existing.Topology = dto.Topology;
            existing.TypeOperation = dto.TypeOperation;
            existing.Area = dto.Area;
            existing.Floor = dto.Floor;
            existing.NumberOfFloors = dto.NumberOfFloors;
            existing.Bedrooms = dto.Bedrooms;
            existing.Bathrooms = dto.Bathrooms;
            existing.HasGarage = dto.HasGarage;
            existing.HasElevator = dto.HasElevator;
            existing.BuiltAt = dto.BuiltAt;
            existing.AirConditioning = dto.AirConditioning;
            existing.Furnished = dto.Furnished;
            existing.Balcony = dto.Balcony;
            existing.Terrace = dto.Terrace;
            existing.Latitude = dto.Latitude;
            existing.Longitude = dto.Longitude;
            existing.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            await NotifyPropertyChangesAsync(existing, oldPrice, oldStatus);
            AwardPointsToAgentIfCompleted(existing, oldStatus);

            return new PropertyDTO
            {
                PropertyId = existing.PropertyId,
                Title = existing.Title,
                Price = existing.Price,
                Status = existing.Status,
                Street = existing.Street,
                PostalCode = existing.PostalCode,
                Area = existing.Area,
                Topology = existing.Topology,
                TypeOperation = existing.TypeOperation,
                AgentId = existing.AgentId,
            };
        }

        public bool DeleteProperty(int idProperty, int userId)
        {
            var property = context.Properties.FirstOrDefault(p => p.PropertyId == idProperty);

            if (property == null)
                throw new PropertyNotFoundException("Property was not found.");

            if (property.AgentId != userId)
                throw new AgentNotAuthorizedException("You are not allowed to delete this property.");

            if (property.Status == Status.Deleted)
                throw new InvalidOperationException("Property is already deleted.");

            var oldStatus = property.Status;
            property.Status = Status.Deleted;
            property.UpdatedAt = DateTime.UtcNow;

            context.SaveChanges();

            return true;
        }

        public IEnumerable<PropertyListDTO> GetPropertiesOf(int idAgent)
        {
            var agent = context.Users.Any(u => u.UserId == idAgent && u.Role == Role.Agent);

            if (!agent)
                throw new AgentNotFoundException("Agent was not found!");

            var properties = context.Properties
                .Where(p => p.AgentId == idAgent)
                .Select(p => new PropertyListDTO
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    Price = p.Price,
                    Topology = p.Topology,
                    Pictures = p.Pictures.Select(p => new PictureDTO
                    {
                        PictureId = p.PictureId,
                        ImagePath = p.ImagePath
                    }).ToList(),
                    AgentId = p.AgentId,
                    UpdatedAt = p.UpdatedAt
                })
                .ToList();

            if (properties.IsNullOrEmpty())
                throw new PropertyNotFoundException("This agent as no properties!");

            return properties;
        }
        public PropertyDTO GetPropertyDetails(int idProperty)
        {
            var propertyDTO = context.Properties.Where(p => p.PropertyId == idProperty)
                .Select(p => new PropertyDTO
                {
                    PropertyId = p.PropertyId,
                    AgentId = p.AgentId,
                    AgentFirstName = p.Agent.FirstName,
                    AgentLastName = p.Agent.LastName,
                    Street = p.Street,
                    PostalCode = p.PostalCode,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Status = p.Status,
                    Topology = p.Topology,
                    TypeOperation = p.TypeOperation,
                    Pictures = p.Pictures.Select(p => new PictureDTO
                    {
                        PictureId = p.PictureId,
                        ImagePath = p.ImagePath
                    }).ToList(),
                    Area = p.Area,
                    Floor = p.Floor,
                    NumberOfFloors = p.NumberOfFloors,
                    Bathrooms = p.Bathrooms,
                    Bedrooms = p.Bedrooms,
                    HasGarage = p.HasGarage,
                    HasElevator = p.HasElevator,
                    CreatedAt = p.CreatedAt
                }).FirstOrDefault();

            if (propertyDTO == null)
                throw new PropertyNotFoundException("Property was not found!");

            return propertyDTO;
        }
        
        private string CapitalizeWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var words = input.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', words.Select(w => char.ToUpper(w[0]) + w[1..]));
        }
        private async Task NotifyPropertyChangesAsync(Property existing, decimal oldPrice, Status oldStatus)
        {
            if (existing.Price != oldPrice)
            {
                string message;

                if (existing.Price > oldPrice)
                {
                    message = $"The price of the property '{existing.Title}' increased from {oldPrice:N0}€ to {existing.Price:N0}€.";
                }
                else
                {
                    message = $"The price of the property '{existing.Title}' decreased from {oldPrice:N0}€ to {existing.Price:N0}€.";
                }

                await notificationService.CreateAsync(existing.PropertyId, message);
            }

            if (existing.Status != oldStatus)
            {
                string? message = null;

                switch (existing.Status)
                {
                    case Status.Sold:
                        message = $"The property '{existing.Title}' has been sold.";
                        break;

                    case Status.Leased:
                        message = $"The property '{existing.Title}' has been leased.";
                        break;

                    default:
                        break;
                }

                if (message is not null)
                {
                    await notificationService.CreateAsync(existing.PropertyId, message);
                }
            }
        }

        private void AwardPointsToAgentIfCompleted(Property property, Status oldStatus)
        {
            if ((oldStatus != Status.Sold && property.Status == Status.Sold) ||
                (oldStatus != Status.Leased && property.Status == Status.Leased))
            {
                int points = 0;

                if (property.TypeOperation == TypeOperations.Sale)
                {
                    if (property.Price <= 150000) points = 80;
                    else if (property.Price <= 300000) points = 100;
                    else if (property.Price <= 600000) points = 120;
                    else points = 140;
                }
                else if (property.TypeOperation == TypeOperations.Lease)
                {
                    if (property.Price <= 500) points = 40;
                    else if (property.Price <= 1000) points = 50;
                    else if (property.Price <= 2000) points = 60;
                    else points = 70;
                }

                agentService.AddPoints(property.AgentId, points);
            }
        }
    }
}
