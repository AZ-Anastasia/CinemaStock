using CinemaStock.Shared;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using CinemaStock.Shared.Resources.UserErrors;
using CinemaStock.Shared.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaStock.API.Controllers;

[ApiController]
[Route(ApiRoutes.Admin.CinemaContentEndpoint)]
public class CinemaContentController : ControllerBase
{
    private readonly ICinemaContentService _cinemaService;
    private readonly ILogger<CinemaContentController> _logger;
    private IWebHostEnvironment _env;

    public CinemaContentController(IWebHostEnvironment env, ICinemaContentService cinemaService, ILogger<CinemaContentController> logger)
    {
        _env = env;
        _cinemaService = cinemaService;
        _logger = logger;
    }

    [HttpGet(ApiRoutes.Admin.GetCinemaEndpoint)]
    public async Task<ActionResult<List<CinemaContentResponse>>> GetCinemaContentsListAsync(
        [FromQuery] string? filter,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 15)
    {
        try
        {
            var data = await _cinemaService.GetCinemaContentsListAsync(filter, pageNumber, pageSize);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при чтении записей из таблицы MediaContents.");
            return StatusCode(500, UserErrors.ReadingError);
        }
    }

    [HttpGet(ApiRoutes.Admin.GetCinemaSearchLiveEndpoint)]
    public async Task<ActionResult> GetCinemaContentsSearchLiveListAsync([FromQuery] string query, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 3)
                return BadRequest(UserErrors.RequestQueryLengthError);
            
            var result = await _cinemaService.GetCinemaContentsSearchLiveListAsync(query.Trim(), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при чтении записей из таблицы MediaContents.");
            return StatusCode(500, UserErrors.ReadingError);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost(ApiRoutes.Admin.CreateCinemaEndpoint)]
    [ProducesResponseType(typeof(CinemaContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CinemaContentResponse>> CreateCinemaContentAsync([FromForm] CreateCinemaContentRequest request)
    // public async Task<IActionResult> CreateMediaContent([FromBody] CreateMediaContentRequest request)
    {
        _logger.LogInformation("Получен HTTP POST запрос на создание фильма");

        if (Request == null)
            return BadRequest(UserErrors.EmptyBodyError);

        try
        {
            if (request.Picture != null)
                // Проверяем: это новый файл в формате Base64 или старый URL?
                if (request.Picture.StartsWith("data:image/"))
                {
                    // 1. Безопасно извлекаем расширение из mime-типа (data:image/png;base64,...)
                    var mimeType = request.Picture.Split(';')[0].Split(':')[1]; // Получим "image/png" или "image/jpeg"
                    var extension = mimeType == "image/jpeg" ? ".jpg" : $".{mimeType.Split('/')[1]}";

                    // 2. Отсекаем заголовок и получаем чистый Base64
                    var base64Raw = request.Picture.Split(',')[1];
                    var fileBytes = Convert.FromBase64String(base64Raw);

                    // 3. ЗАЩИТА DoS: Проверяем размер массива байт на сервере
                    if (fileBytes.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(UserErrors.TooLargeFileSize);
                    }

                    // 4. ЗАЩИТА Path Traversal: Генерируем случайное имя на сервере
                    var secureFileName = $"{Guid.NewGuid()}{extension}";
                    var uploadFolder = Path.Combine(_env.ContentRootPath, ConstStrings.DefaultFilmImageDirectoryPath);
                    var uploadPath = Path.Combine(uploadFolder, secureFileName);

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // 5. Безопасное сохранение
                    using (var fileStream = new FileStream(uploadPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                        await fileStream.WriteAsync(fileBytes, 0, fileBytes.Length);

                    request.Picture = $"{ConstStrings.DefaultFilmImageDirectoryPath}/{secureFileName}";
                }

            var createdMedia = await _cinemaService.CreateCinemaContentAsync(request);
            return StatusCode(201, createdMedia);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"{UserErrors.PostError}: {ex}.");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CinemaContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CinemaContentResponse>> UpdateCinemaContentAsync([FromRoute] Guid id, [FromForm] CreateCinemaContentRequest request)
    {
        _logger.LogInformation("Получен HTTP POST запрос на изменение фильма с ID: {Id}", id);

        if (Request == null)
            return BadRequest(UserErrors.EmptyBodyError);

        try
        {
            if (request.Picture != null)
                // Проверяем: это новый файл в формате Base64 или старый URL?
                if (request.Picture.StartsWith("data:image/"))
                {
                    // 1. Безопасно извлекаем расширение из mime-типа (data:image/png;base64,...)
                    var mimeType = request.Picture.Split(';')[0].Split(':')[1]; // Получим "image/png" или "image/jpeg"
                    var extension = mimeType == "image/jpeg" ? ".jpg" : $".{mimeType.Split('/')[1]}";

                    // 2. Отсекаем заголовок и получаем чистый Base64
                    var base64Raw = request.Picture.Split(',')[1];
                    var fileBytes = Convert.FromBase64String(base64Raw);

                    // 3. ЗАЩИТА DoS: Проверяем размер массива байт на сервере
                    if (fileBytes.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(UserErrors.TooLargeFileSize);
                    }

                    // 4. ЗАЩИТА Path Traversal: Генерируем случайное имя на сервере
                    var secureFileName = $"{Guid.NewGuid()}{extension}";
                    var uploadFolder = Path.Combine(_env.ContentRootPath, ConstStrings.DefaultFilmImageDirectoryPath);
                    var uploadPath = Path.Combine(uploadFolder, secureFileName);

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // 5. Безопасное сохранение
                    using (var fileStream = new FileStream(uploadPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                        await fileStream.WriteAsync(fileBytes, 0, fileBytes.Length);

                    request.Picture = $"{ConstStrings.DefaultFilmImageDirectoryPath}/{secureFileName}";
                }

            var updatedMedia = await _cinemaService.UpdateCinemaContentAsync(id, request);

            if (updatedMedia == null)
                return NotFound(UserErrors.NoContentInDB);

            return StatusCode(201, updatedMedia);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"{UserErrors.UpdatingError}: {ex}.");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCinemaContentAsync([FromRoute] Guid id)
    {
        try
        {
            var isDeleted = await _cinemaService.DeleteCinemaContentAsync(id);
            if (!isDeleted)
            {
                _logger.LogWarning("Попытка удаления: Киноконтент с ID {Id} не найден.",id);
                return NotFound(UserErrors.NoContentInDB);
            }

            _logger.LogInformation("Киноконтент с ID {Id} успешно удален.",id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой API при удалении записи из таблицы MediaContents.");
            return StatusCode(500, UserErrors.DeletingError);
        }
    }
}