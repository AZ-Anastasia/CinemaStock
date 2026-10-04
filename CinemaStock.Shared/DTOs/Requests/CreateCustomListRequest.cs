using System.ComponentModel.DataAnnotations;

namespace CinemaStock.Shared.DTOs.Requests;

public class CreateCustomListRequest
{
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}