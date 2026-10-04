using CinemaStock.API.Data;
using CinemaStock.API.Data.Entities.Base;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace CinemaStock.API.Services;

public class GenreService : IGenreService
{
    private readonly AppDbContext _context;

    public GenreService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<IdNameResponse>> GetGenresListAsync()
    {
        return await _context.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new IdNameResponse
            {
                Id = g.Id,
                Name = g.Name
            })
            .ToListAsync();
    }

    public async Task<IdNameResponse?> CreateGenreAsync(CreateGenreOrTagRequest request)
    {
        var trimmedName = request.Name.Trim();

        var isExist = await _context.Genres.AnyAsync(g => g.Name.ToLower() == trimmedName.ToLower());
        if (isExist)
            return null;

        var newGenreId = Guid.NewGuid();
        var genre = new Genre { Id = newGenreId, Name = request.Name };

        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

        return new IdNameResponse
        {
            Id = newGenreId,
            Name = trimmedName
        };
    }

    public async Task<IdNameResponse?> UpdateGenreAsync(Guid id, GenreTagRequest request)
    {
        var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Id == id);
        if (genre == null)
            return null;

        var trimmedName = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            return null;

        var isNameTaken = await _context.Genres.AnyAsync(g =>
            g.Id != id &&
            g.Name.ToLower() == trimmedName.ToLower());

        if (isNameTaken)
            return null;

        genre.Name = trimmedName;

        _context.Genres.Update(genre);
        await _context.SaveChangesAsync();

        return new IdNameResponse
        {
            Id = genre.Id,
            Name = genre.Name
        };
    }

    public async Task<bool> DeleteGenreAsync(Guid id)
    {
        var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Id == id);
        if (genre == null)
            return false;

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();

        return true;
    }
}