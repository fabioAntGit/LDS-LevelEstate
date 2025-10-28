using API.FavoritosNotificacoes.Favoritos.Interfaces;
using API.Imoveis.Exceptions;
using API.Imoveis.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.FavoritosNotificacoes.Favoritos.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            this.favoriteService = favoriteService;
        }


        [HttpPost("{propertyId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> AddFavorite(int propertyId)
        {
            int idClient = 1; // AUTH NOT IMPLEMENTED YET
            try
            {
                this.favoriteService.AddFavorite(propertyId, idClient);
                return Ok();
            }
            catch (PropertyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> RemoveFavorite(int propertyId)
        {
            int idClient = 1; // AUTH NOT IMPLEMENTED YET
            try
            {
                this.favoriteService.RemoveFavorite(propertyId, idClient);
                return Ok();
            }
            catch (PropertyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PropertyDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<PropertyDTO>> ListFavorites()
        {
            int idClient = 1; // AUTH NOT IMPLEMENTED YET
            try
            {
                var favorites = this.favoriteService.ListFavorites(idClient);
                return Ok(favorites);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
