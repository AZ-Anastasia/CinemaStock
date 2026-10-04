using CinemaStock.API.Data.Entities;
using CinemaStock.API.Data.Entities.Base;
using CinemaStock.Shared;
using CinemaStock.Shared.DTOs.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaStock.API.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<MediaContent> MediaContents { get; set; }
    public DbSet<CinemaContent> CinemaContents { get; set; }
    public DbSet<Game> Games { get; set; }

    public DbSet<PlatformOS> PlatformsOS { get; set; }
    public DbSet<Company> Companies { get; set; }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<UserMediaProgress> UserMediaProgress { get; set; }
    public DbSet<UserCinemaProgress> UserCinemaProgress { get; set; }
    public DbSet<UserGameProgress> UserGameProgress { get; set; }
    public DbSet<CustomList> CustomLists { get; set; }

    /// <summary>
    /// Конфигурирование схемы БД, связи между сущностями и правила валидации
    /// перед созданием и инициализацией контекста БД
    /// </summary>
    /// <param name="modelBuilder">Строитель моделей, использующийся для настройки конфигурации сущностей схем</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Genre>()
            .HasIndex(g => g.Name)
            .IsUnique();
        
        modelBuilder.Entity<Genre>()
            .Property(c => c.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();
        
        modelBuilder.Entity<Tag>()
            .Property(c => c.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<PlatformOS>()
            .HasIndex(p => p.Name)
            .IsUnique();

        modelBuilder.Entity<PlatformOS>()
            .Property(c => c.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Company>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Company>()
            .Property(c => c.Name)
            .HasMaxLength(255);

        modelBuilder.Entity<ReleaseFormat>()
            .Property(c => c.FormatName)
            .HasMaxLength(50);

        modelBuilder.Entity<CustomList>()
            .Property(c => c.Name)
            .HasMaxLength(255);

        modelBuilder.Entity<UserCinemaProgress>()
            .Property(c => c.WatchStatus)
            .HasMaxLength(50);

        modelBuilder.Entity<UserGameProgress>()
            .Property(c => c.PlayStatus)
            .HasMaxLength(50);

        modelBuilder.Entity<MediaContent>(entity =>
        {
            // Настройка паттерна TPH - Table-per-Hierarchy - (EF создаст одну общую таблицу "MediaContents"
            // и добавит колонку "Discriminator", чтобы отличать фильмы от игр)
            entity.UseTphMappingStrategy();

            entity.Property(m => m.Picture)
                .HasDefaultValue(ConstStrings.DefaultFilmImagePath);

            entity.Property(m => m.Rating)
                .HasDefaultValue(0.0m);

            entity.Property(m => m.Type)
                .HasDefaultValue(ReleaseType.Unknown);

            entity.Property(m => m.LocalTitle)
                .HasMaxLength(255);

            entity.Property(m => m.OriginalTitle)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<CinemaContent>(entity =>
        {
            entity.HasMany(c => c.CinemaGenres)
                .WithMany(g => g.CinemaContents);

            entity.HasMany(c => c.CinemaTags)
                .WithMany(t => t.CinemaContents);

            entity.HasOne(c => c.ReleaseStudio)
                .WithMany(co => co.CinemaContents)
                .HasForeignKey(c => c.ReleaseStudioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasMany(g => g.GameGenres)
                .WithMany(gen => gen.Games);

            entity.HasMany(g => g.GameTags)
                .WithMany(t => t.Games);

            entity.HasOne(g => g.Developer)
                .WithMany(c => c.DevelopedGames)
                .HasForeignKey(g => g.DeveloperId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(g => g.Publisher)
                .WithMany(c => c.PublishedGames)
                .HasForeignKey(g => g.PublisherId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(g => g.Platform)
                .WithMany(p => p.Games);
        });

        modelBuilder.Entity<UserMediaProgress>(entity=>
        {
            entity.UseTphMappingStrategy();

            entity.HasOne(ump => ump.User)
                .WithMany(u => u.MediaProgress)
                .HasForeignKey(ump => ump.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserGameProgress>()
            .Property(p => p.PlayStatus)
            .HasConversion<string>();

        modelBuilder.Entity<UserCinemaProgress>()
            .Property(p => p.WatchStatus)
            .HasConversion<string>();

        #region ReleaseType.Movie -> "Movie"

        var releaseFormats = Enum.GetValues<ReleaseType>()
            .Select(e => new ReleaseFormat
            {
                Id = e,
                FormatName = e.ToString()
            })
            .ToArray();

        modelBuilder.Entity<ReleaseFormat>().HasData(releaseFormats);

        #endregion
    }
}