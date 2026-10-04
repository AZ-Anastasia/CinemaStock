using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.Resources.UserErrors;
using CinemaStock.Shared.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaStock.API.Controllers;

[ApiController]
[Route(ApiRoutes.Admin.GenresControllerName)]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;
    private readonly ILogger<GenresController> _logger;

    public GenresController(IGenreService genreService, ILogger<GenresController> logger)
    {
        _genreService = genreService;
        _logger = logger;
    }

    [HttpGet(ApiRoutes.Admin.ListEndpoint)]
    public async Task<IActionResult> GetGenresList()
    {
        try
        {
            var genres = await _genreService.GetGenresListAsync();
            return Ok(genres);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при чтении таблицы Genres из PostgreSQL.");
            return StatusCode(500, UserErrors.GenreReadingError);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateGenre([FromBody] CreateGenreOrTagRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(UserErrors.GenreNameRequired);
        }

        try
        {
            var result = await _genreService.CreateGenreAsync(request);
            if (result == null)
                return BadRequest(UserErrors.GenreAlreadyExist);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при добавлении записи в таблицу Genres.");
            return StatusCode(500, UserErrors.GenrePostingError);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateGenre([FromRoute] Guid id, [FromBody] GenreTagRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(UserErrors.GenreNameRequired);
        }
        try
        {
            var updatedGenre = await _genreService.UpdateGenreAsync(id, request);
            if (updatedGenre == null)
                return BadRequest(UserErrors.GenreAlreadyExist);

            return Ok(updatedGenre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при обновлении записи в таблицу Genres.");
            return StatusCode(500, UserErrors.GenreUpdatingError);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteGenre([FromRoute] Guid id)
    {
        try
        {
            var isDeleted = await _genreService.DeleteGenreAsync(id);

            if (!isDeleted)
                return BadRequest(UserErrors.GenreAlreadyExist);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при удалении записи в таблицу Genres.");
            return StatusCode(500, UserErrors.GenreDeletingError);
        }
    }
}