using CinemaStock.Shared.DTOs.Enums;

namespace CinemaStock.API.Data.Entities.Base;

public class UserCinemaProgress : UserMediaProgress
{
    public int EpisodeStopWatching { get; set; } = 0;
    public WatchStatus WatchStatus { get; set; } = WatchStatus.PlanToWatch;
}