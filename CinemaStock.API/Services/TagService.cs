using CinemaStock.API.Data;
using CinemaStock.API.Data.Entities.Base;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace CinemaStock.API.Services;

public class TagService : ITagService
{
    private readonly AppDbContext _context;

    public TagService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<IdNameResponse>> GetTagsListAsync()
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new IdNameResponse
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToListAsync();
    }

    public async Task<IdNameResponse?> CreateTagAsync(CreateGenreOrTagRequest request)
    {
        var trimmedName = request.Name.Trim();

        var isExist = await _context.Tags.AnyAsync(t => t.Name.ToLower() == trimmedName.ToLower());
        if (isExist)
            return null;

        var newTagId = Guid.NewGuid();
        var tag = new Tag { Id = newTagId, Name = request.Name };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return new IdNameResponse
        {
            Id = newTagId,
            Name = trimmedName
        };
    }

    public async Task<IdNameResponse?> UpdateTagAsync(Guid id, GenreTagRequest request)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (tag == null)
            return null;

        var trimmedName = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            return null;

        var isNameTaken = await _context.Tags.AnyAsync(t =>
            t.Id != id &&
            t.Name.ToLower() == trimmedName.ToLower());

        if (isNameTaken)
            return null;

        tag.Name = trimmedName;

        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();

        return new IdNameResponse
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }

    public async Task<bool> DeleteTagAsync(Guid id)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (tag == null)
            return false;

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        return true;
    }
}