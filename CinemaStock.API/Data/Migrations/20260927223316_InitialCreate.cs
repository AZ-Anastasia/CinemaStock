using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaStock.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformsOS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformsOS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseFormat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    FormatName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseFormat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomLists_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeveloperId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublisherId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Companies_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Games_Companies_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MediaContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OriginalTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Picture = table.Column<string>(type: "text", nullable: true, defaultValue: "images/films/default/default-cover.png"),
                    Type = table.Column<int>(type: "integer", nullable: true, defaultValue: 4),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0.0m),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SourceInfoFrom = table.Column<string>(type: "text", nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    ReleaseFormatId = table.Column<int>(type: "integer", nullable: true),
                    DurationInMinutes = table.Column<int>(type: "integer", nullable: true),
                    ReleaseStudioId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaContents_Companies_ReleaseStudioId",
                        column: x => x.ReleaseStudioId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MediaContents_ReleaseFormat_ReleaseFormatId",
                        column: x => x.ReleaseFormatId,
                        principalTable: "ReleaseFormat",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GameGenre",
                columns: table => new
                {
                    GameGenresId = table.Column<Guid>(type: "uuid", nullable: false),
                    GamesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameGenre", x => new { x.GameGenresId, x.GamesId });
                    table.ForeignKey(
                        name: "FK_GameGenre_Games_GamesId",
                        column: x => x.GamesId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameGenre_Genres_GameGenresId",
                        column: x => x.GameGenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GamePlatformOS",
                columns: table => new
                {
                    GamesId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlatformOS", x => new { x.GamesId, x.PlatformId });
                    table.ForeignKey(
                        name: "FK_GamePlatformOS_Games_GamesId",
                        column: x => x.GamesId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePlatformOS_PlatformsOS_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "PlatformsOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameTag",
                columns: table => new
                {
                    GameTagsId = table.Column<Guid>(type: "uuid", nullable: false),
                    GamesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameTag", x => new { x.GameTagsId, x.GamesId });
                    table.ForeignKey(
                        name: "FK_GameTag_Games_GamesId",
                        column: x => x.GamesId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameTag_Tags_GameTagsId",
                        column: x => x.GameTagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CinemaContentGenre",
                columns: table => new
                {
                    CinemaContentsId = table.Column<Guid>(type: "uuid", nullable: false),
                    CinemaGenresId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CinemaContentGenre", x => new { x.CinemaContentsId, x.CinemaGenresId });
                    table.ForeignKey(
                        name: "FK_CinemaContentGenre_Genres_CinemaGenresId",
                        column: x => x.CinemaGenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CinemaContentGenre_MediaContents_CinemaContentsId",
                        column: x => x.CinemaContentsId,
                        principalTable: "MediaContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CinemaContentTag",
                columns: table => new
                {
                    CinemaContentsId = table.Column<Guid>(type: "uuid", nullable: false),
                    CinemaTagsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CinemaContentTag", x => new { x.CinemaContentsId, x.CinemaTagsId });
                    table.ForeignKey(
                        name: "FK_CinemaContentTag_MediaContents_CinemaContentsId",
                        column: x => x.CinemaContentsId,
                        principalTable: "MediaContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CinemaContentTag_Tags_CinemaTagsId",
                        column: x => x.CinemaTagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMediaProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MediaContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnRate = table.Column<double>(type: "double precision", nullable: true),
                    MyComment = table.Column<string>(type: "text", nullable: false),
                    CinemaContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    EpisodeStopWatching = table.Column<int>(type: "integer", nullable: true),
                    UserCinemaProgress_Status = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMediaProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMediaProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMediaProgress_MediaContents_CinemaContentId",
                        column: x => x.CinemaContentId,
                        principalTable: "MediaContents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserMediaProgress_MediaContents_MediaContentId",
                        column: x => x.MediaContentId,
                        principalTable: "MediaContents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomListUserMediaProgress",
                columns: table => new
                {
                    CustomListId = table.Column<int>(type: "integer", nullable: false),
                    UserMediaProgressesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomListUserMediaProgress", x => new { x.CustomListId, x.UserMediaProgressesId });
                    table.ForeignKey(
                        name: "FK_CustomListUserMediaProgress_CustomLists_CustomListId",
                        column: x => x.CustomListId,
                        principalTable: "CustomLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomListUserMediaProgress_UserMediaProgress_UserMediaProg~",
                        column: x => x.UserMediaProgressesId,
                        principalTable: "UserMediaProgress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ReleaseFormat",
                columns: new[] { "Id", "FormatName" },
                values: new object[,]
                {
                    { 0, "Movie" },
                    { 1, "TV" },
                    { 2, "OVA" },
                    { 3, "ONA" },
                    { 4, "Unknown" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CinemaContentGenre_CinemaGenresId",
                table: "CinemaContentGenre",
                column: "CinemaGenresId");

            migrationBuilder.CreateIndex(
                name: "IX_CinemaContentTag_CinemaTagsId",
                table: "CinemaContentTag",
                column: "CinemaTagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomLists_UserId",
                table: "CustomLists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomListUserMediaProgress_UserMediaProgressesId",
                table: "CustomListUserMediaProgress",
                column: "UserMediaProgressesId");

            migrationBuilder.CreateIndex(
                name: "IX_GameGenre_GamesId",
                table: "GameGenre",
                column: "GamesId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlatformOS_PlatformId",
                table: "GamePlatformOS",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_DeveloperId",
                table: "Games",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_PublisherId",
                table: "Games",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_GameTag_GamesId",
                table: "GameTag",
                column: "GamesId");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Name",
                table: "Genres",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaContents_ReleaseFormatId",
                table: "MediaContents",
                column: "ReleaseFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaContents_ReleaseStudioId",
                table: "MediaContents",
                column: "ReleaseStudioId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformsOS_Name",
                table: "PlatformsOS",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMediaProgress_CinemaContentId",
                table: "UserMediaProgress",
                column: "CinemaContentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMediaProgress_MediaContentId",
                table: "UserMediaProgress",
                column: "MediaContentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMediaProgress_UserId",
                table: "UserMediaProgress",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CinemaContentGenre");

            migrationBuilder.DropTable(
                name: "CinemaContentTag");

            migrationBuilder.DropTable(
                name: "CustomListUserMediaProgress");

            migrationBuilder.DropTable(
                name: "GameGenre");

            migrationBuilder.DropTable(
                name: "GamePlatformOS");

            migrationBuilder.DropTable(
                name: "GameTag");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "CustomLists");

            migrationBuilder.DropTable(
                name: "UserMediaProgress");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "PlatformsOS");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "MediaContents");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "ReleaseFormat");
        }
    }
}
