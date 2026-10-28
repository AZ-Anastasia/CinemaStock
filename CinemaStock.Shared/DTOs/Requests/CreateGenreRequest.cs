using System.ComponentModel.DataAnnotations;

namespace CinemaStock.Shared.DTOs.Requests;

public class CreateGenreOrTagRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
}