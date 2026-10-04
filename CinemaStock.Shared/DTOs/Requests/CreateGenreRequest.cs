using System.ComponentModel.DataAnnotations;

namespace CinemaStock.Shared.DTOs.Requests;

public class CreateGenreOrTagRequest
{
    [Required]
    public string Name { get; set; } = null!;
}