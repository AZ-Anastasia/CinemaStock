using CinemaStock.API.Data;
using CinemaStock.API.Data.Entities;
using CinemaStock.Shared;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Enums;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace CinemaStock.API.Services;

public class CinemaContentService : ICinemaContentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CinemaContentService> _logger;

    public CinemaContentService(AppDbContext db, ILogger<CinemaContentService> logger)
    {
        _context = db;
        _logger = logger;
    }

    public async Task<CinemaContentResponse> CreateCinemaContentAsync(CreateCinemaContentRequest request)
    {
        var studioName = request.ReleaseStudio?.Trim().ToLower();
        Company? existingCompany = null;

        if (!string.IsNullOrWhiteSpace(studioName))
        {
            existingCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name.ToLower() == studioName);

            if (existingCompany == null)
            {
                existingCompany = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = request.ReleaseStudio!.Trim()
                };
                _context.Companies.Add(existingCompany);
            }
        }

        var genreIds = request.GenresIds ?? new List<Guid>();
        var tagIds = request.TagsIds ?? new List<Guid>();

        var existingGenres = await _context.Genres
            .Where(g => genreIds.Contains(g.Id))
            .ToListAsync();

        var existingTags = await _context.Tags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync();

        var newCinema = new CinemaContent
        {
            Id = Guid.NewGuid(),
            OriginalTitle = request.OriginalTitle!.Trim(),
            LocalTitle = request.LocalTitle?.Trim(),
            Picture = request.Picture,
            Type = request.Type ?? ReleaseType.Unknown,
            ReleaseDate = request.ReleaseDate,
            Description = request.Description?.Trim(),
            ReleaseStudio = existingCompany,
            CinemaGenres = existingGenres,
            CinemaTags = existingTags
        };

        try
        {
            // Добавляем в контекст и сохраняем в Postgres
            _context.MediaContents.Add(newCinema);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Медиаконтент успешно сохранен в БД. Назначен ID: {MediaId}", newCinema.Id);

            return new CinemaContentResponse(
                newCinema.Id,
                newCinema.LocalTitle,
                newCinema.OriginalTitle,
                newCinema.Picture ?? ConstStrings.DefaultFilmImagePath,
                newCinema.Type.ToString()!,
                newCinema.ReleaseDate,
                newCinema.Rating,
                newCinema.Description,
                newCinema.ReleaseStudio != null
                    ? new IdNameResponse
                    {
                        Id = newCinema.ReleaseStudio.Id,
                        Name = newCinema.ReleaseStudio.Name
                    }
                    : null,
                newCinema.CinemaGenres.Select(g => g.Name).ToList(),
                newCinema.CinemaTags.Select(t => t.Name).ToList()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка при сохранении медиаконтента {Title} в БД", request.OriginalTitle);
            throw;
        }
    }

    public async Task<CinemaContentResponse?> UpdateCinemaContentAsync(Guid id, CreateCinemaContentRequest request)
    {
        var cinema = await _context.CinemaContents
            .Include(c => c.CinemaGenres)
            .Include(c => c.CinemaTags)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cinema == null) return null;

        var studioName = request.ReleaseStudio?.Trim().ToLower();
        Company? existingCompany = null;

        if (!string.IsNullOrWhiteSpace(studioName))
        {
            existingCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name.ToLower() == studioName);

            if (existingCompany == null)
            {
                existingCompany = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = request.ReleaseStudio!.Trim()
                };
                _context.Companies.Add(existingCompany);
            }
        }

        var genreIds = request.GenresIds ?? new List<Guid>();
        var tagIds = request.TagsIds ?? new List<Guid>();

        var existingGenres = await _context.Genres
            .Where(g => genreIds.Contains(g.Id))
            .ToListAsync();

        var existingTags = await _context.Tags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync();

        cinema.OriginalTitle = request.OriginalTitle!.Trim();
        cinema.LocalTitle = request.LocalTitle?.Trim();
        cinema.Picture = request.Picture;
        cinema.Type = request.Type ?? ReleaseType.Unknown;
        cinema.ReleaseDate = request.ReleaseDate;
        cinema.Description = request.Description?.Trim();
        cinema.ReleaseStudio = existingCompany;
        cinema.CinemaGenres = existingGenres;
        cinema.CinemaTags = existingTags;

        if (!string.IsNullOrWhiteSpace(request.Picture) && request.Picture != cinema.Picture)
            cinema.Picture = request.Picture;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Медиаконтент успешно обновлен в БД. Назначен ID: {MediaId}", cinema.Id);

        return new CinemaContentResponse
        (
            cinema.Id,
                cinema.LocalTitle,
                cinema.OriginalTitle,
                cinema.Picture ?? ConstStrings.DefaultFilmImagePath,
                cinema.Type.ToString()!,
                cinema.ReleaseDate,
                cinema.Rating,
                cinema.Description,
                cinema.ReleaseStudio != null
                    ? new IdNameResponse
                    {
                        Id = cinema.ReleaseStudio.Id,
                        Name = cinema.ReleaseStudio.Name
                    }
                    : null,
                cinema.CinemaGenres.Select(g => g.Name).ToList(),
                cinema.CinemaTags.Select(t => t.Name).ToList()
        );
    }

    public async Task<List<CinemaContentResponse>> GetCinemaContentsListAsync(string? filter, int pageNumber, int pageSize)
    {
        int skipCount = (pageNumber - 1) * pageSize;

        var query = _context.CinemaContents
            .Include(m => m.CinemaGenres)
            .Include(m => m.CinemaTags)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var lowerFilter = filter.ToLower();
            query = query.Where(m =>
                (m.LocalTitle != null && m.LocalTitle.ToLower().Contains(lowerFilter))
                    || m.OriginalTitle.ToLower().Contains(lowerFilter));
        }

        var result = await query
            .OrderByDescending(m => m.ReleaseDate)
            .Skip(skipCount)
            .Take(pageSize)
            .Select(m => new CinemaContentResponse(
                m.Id,
                m.LocalTitle,
                m.OriginalTitle,
                m.Picture,
                m.Type.ToString()!,
                m.ReleaseDate,
                m.Rating,
                m.Description,
                m.ReleaseStudio != null
                    ? new IdNameResponse
                    {
                        Id = m.ReleaseStudio.Id,
                        Name = m.ReleaseStudio.Name
                    }
                    : null,
                m.CinemaGenres.Select(g => g.Name).ToList(),
                m.CinemaTags.Select(t => t.Name).ToList()
            )).ToListAsync();

        return result;
    }

    public async Task<bool> DeleteCinemaContentAsync(Guid id)
    {
        var cinema = await _context.MediaContents.FirstOrDefaultAsync(x => x.Id == id);

        if (cinema == null)
        {
            return false;
        }

        _context.MediaContents.Remove(cinema);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CinemaContentResponse>> GetCinemaContentsSearchLiveListAsync(string query, CancellationToken ct)
    {
        var rawData = await _context.CinemaContents
            .Where(c => EF.Functions.ILike(c.LocalTitle!, $"%{query}%")
                || EF.Functions.ILike(c.OriginalTitle, $"%{query}%"))
            .Take(5)
            .Select(c => new
            {
                c.Id,
                c.LocalTitle,
                c.OriginalTitle,
                c.Picture,
                Type = c.Type != null ? c.Type.ToString() : ReleaseType.Unknown.ToString(),
                c.ReleaseDate,
                c.Rating,
                c.Description,

                StudioId = c.ReleaseStudio != null ? (Guid?)c.ReleaseStudio.Id : null,
                StudioName = c.ReleaseStudio != null ? c.ReleaseStudio.Name : null,

                Genres = c.CinemaGenres.Select(g => g.Name).ToList(),
                Tags = c.CinemaTags.Select(t => t.Name).ToList()
            }).ToListAsync(ct);
        return rawData.Select(c => new CinemaContentResponse
        (
            c.Id,
            c.LocalTitle,
            c.OriginalTitle,
            c.Picture,
            c.Type!,
            c.ReleaseDate,
            c.Rating,
            c.Description,
            c.StudioId.HasValue ? new IdNameResponse()
                {
                    Id = c.StudioId.Value,
                    Name = c.StudioName ?? string.Empty
                }
                : null,
            c.Genres,
            c.Tags
        )).ToList();
    }
}