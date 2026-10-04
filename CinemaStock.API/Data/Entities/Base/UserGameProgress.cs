using CinemaStock.Shared.DTOs.Enums;

namespace CinemaStock.API.Data.Entities.Base;

public class UserGameProgress : UserMediaProgress
{
    public PlayStatus PlayStatus { get; set; } = PlayStatus.PlanToPlay;
}