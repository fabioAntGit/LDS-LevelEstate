using API.APIs.Exceptions;
using API.APIs.Interfaces;
using API.Models.Enums;
using API.PesquisasFiltros.Pesquisas.DTOs;
using API.PesquisasFiltros.Pesquisas.Exceptions;
using API.PesquisasFiltros.Pesquisas.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.PesquisasFiltros.Pesquisas.Controllers
{

    [Route("api/property/search")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [ProducesResponseType(typeof(PropertyListDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public class PropertySearchController : ControllerBase
    {
        private readonly IPropertySearchService propertySearchService;
        private readonly IMapboxService mapboxService;

        public PropertySearchController(IPropertySearchService propertySearchService, IMapboxService mapboxService)
        {
            this.propertySearchService = propertySearchService;
            this.mapboxService = mapboxService;
        }

        [HttpGet("filter")]
        public async Task<ActionResult<PropertyListDTO>> GetFilteredProperties([FromQuery] string? address, [FromQuery] double? minPrice,
            [FromQuery] double? maxPrice, [FromQuery] TypeOperations? typeOperation, [FromQuery] Topologies? topology)
        {
            try
            {
                double? latitude = null;
                double? longitude = null;

                if (!string.IsNullOrWhiteSpace(address))
                {
                    var coords = await mapboxService.GetCoordinatesAsync(address);

                    if (coords != null)
                    {
                        latitude = coords.Value.Latitude;
                        longitude = coords.Value.Longitude;
                    }
                }

                var result = propertySearchService.GetPropertiesByFilter(topology, minPrice, maxPrice, typeOperation, latitude, longitude);

                return Ok(result);
            }
            catch (PropertySearchNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (MapboxUnavailableException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (MapboxNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
