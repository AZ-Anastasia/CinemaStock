using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.Resources.UserErrors;
using CinemaStock.Shared.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaStock.API.Controllers;

[ApiController]
[Route(ApiRoutes.Admin.TagsControllerName)]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagService tagService, ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    [HttpGet(ApiRoutes.Admin.ListEndpoint)]
    public async Task<IActionResult> GetTagsList()
    {
        try
        {
            var tags = await _tagService.GetTagsListAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при чтении таблицы Tags из PostgreSQL.");
            return StatusCode(500, UserErrors.TagsReadingError);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTag([FromBody] CreateGenreOrTagRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(UserErrors.TagNameRequired);
        }
        try
        {
            var result = await _tagService.CreateTagAsync(request);
            if (result == null)
                return BadRequest(UserErrors.TagAlreadyExist);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при добавлении записи в таблицу Tags.");
            return StatusCode(500, UserErrors.TagsPostingError);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTag([FromRoute] Guid id, [FromBody] GenreTagRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(UserErrors.TagNameRequired);
        }
        try
        {
            var updatedGenre = await _tagService.UpdateTagAsync(id, request);
            if (updatedGenre == null)
                return BadRequest(UserErrors.TagAlreadyExist);

            return Ok(updatedGenre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при обновлении записи в таблицу Tags.");
            return StatusCode(500, UserErrors.TagsUpdatingError);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTag([FromRoute] Guid id)
    {
        try
        {
            var isDeleted = await _tagService.DeleteTagAsync(id);

            if (!isDeleted)
            return BadRequest(UserErrors.TagNameRequired);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при удалении записи в таблицу Tags.");
            return StatusCode(500, UserErrors.TagsDeletingError);
        }
    }
}