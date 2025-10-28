using API.Imoveis.DTOs;
using API.Imoveis.Exceptions;
using API.Imoveis.Interfaces;
using API.Perfis.Agente.Exceptions;
using API.PesquisasFiltros.Pesquisas.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Imoveis.Controllers
{
    [Route("api/property")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService propertyService;

        public PropertyController(IPropertyService propertyService)
        {
            this.propertyService = propertyService;
        }

        [HttpGet("agent/{idAgent}")]
        [ProducesResponseType(typeof(PropertyListDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PropertyListDTO> GetPropertiesOf(int idAgent)
        {
            try
            {
                var properties = propertyService.GetPropertiesOf(idAgent);

                return Ok(properties);
            }
            catch(AgentNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch(PropertyNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{idProperty}")]
        [ProducesResponseType(typeof(PropertyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PropertyDTO> GetPropertyDetails(int idProperty)
        {
            try
            {
                var property = propertyService.GetPropertyDetails(idProperty);

                return Ok(property);
            }
            catch (PropertyNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(PropertyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PropertyDTO> CreateProperty([FromBody] CreatePropertyDTO dto)
        {
            try
            {
                int userId = 1; // auth not implemented yet

                var newProperty = propertyService.CreateProperty(dto, userId);
                return CreatedAtAction(nameof(GetPropertyDetails), new { idProperty = newProperty.PropertyId }, newProperty);
            }
            catch (ArgumentNullException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch (InvalidPropertyDataException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch (ArgumentException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(PropertyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PropertyDTO>> UpdateProperty([FromBody] UpdatePropertyDTO property)
        {
            try
            {
                int userId = 1; // auth not implemented yet
                var updatedProperty = await propertyService.UpdatePropertyAsync(property, userId);
                return Ok(updatedProperty);
            }
            catch (PropertyNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (InvalidPropertyDataException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch (AgentNotAuthorizedException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (ArgumentException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{idProperty}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteProperty(int idProperty)
        {
            try
            {
                int userId = 1;//auth not implemented yet

                var result = propertyService.DeleteProperty(idProperty, userId);
                return Ok(result);
            }
            catch (PropertyNotFoundException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (AgentNotAuthorizedException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (InvalidOperationException e)
            {
                return Problem(detail: e.Message, statusCode: StatusCodes.Status409Conflict);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
