using API.PesquisasFiltros.Filtros.Exceptions;
using API.PesquisasFiltros.Filtros.Interfaces;
using API.PesquisasFiltros.Pesquisas.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.PesquisasFiltros.Filtros.Controllers
{
    [Route("api/property/search")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [ProducesResponseType(typeof(PropertyListDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class PropertyOrderController : ControllerBase
    {
        private readonly IPropertyOrderService propertyOrderService;

        public PropertyOrderController(IPropertyOrderService propertyOrderService)
        {
            this.propertyOrderService = propertyOrderService;
        }

        [HttpPost("order")]
        public ActionResult<PropertyListDTO> GetOrderProperties([FromBody] IEnumerable<PropertyListDTO> properties,[FromQuery] bool? isPriceDesc, [FromQuery] bool? isDateDesc, 
            [FromQuery] bool? bestAgent)
        {
            try
            {
                var orderedProperties = propertyOrderService.OrderProperties(properties, isPriceDesc, isDateDesc, bestAgent);

                return Ok(orderedProperties);
            }
            catch (PropertyOrderNotFoundException e)
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
