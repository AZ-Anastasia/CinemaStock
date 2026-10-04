using System.ComponentModel.DataAnnotations.Schema;
using CinemaStock.API.Data.Entities.Base;

namespace CinemaStock.API.Data.Entities;

public class CinemaContent : MediaContent
{
    public int DurationInMinutes { get; set; }

    public Guid? ReleaseStudioId { get; set; }
    public Company? ReleaseStudio { get; set; }

    // В БД не добавится, нужно для отображения на страницах
    [NotMapped]
    public string DurationFormatted
    {
        get
        {
            if (DurationInMinutes <= 0) return string.Empty;

            int hours = DurationInMinutes / 60;
            int minutes = DurationInMinutes % 60;

            if (hours > 0 && minutes > 0)
                return $"{hours} ч {minutes} мин";
            if (hours > 0)
                return $"{hours} ч";

            return $"{minutes} мин";
        }
    }

    public List<Genre> CinemaGenres { get; set; } = [];
    public List<Tag> CinemaTags { get; set; } = [];
    public List<UserMediaProgress> UserCinemaProgresses { get; set; } = [];
}