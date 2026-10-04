using System.Security.Claims;
using CinemaStock.API.Data;
using CinemaStock.API.Data.Entities.Base;
using CinemaStock.API.Services;
using CinemaStock.Shared;
using CinemaStock.Shared.ApiEndpointControllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

#region Allowed Origins

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme, options=>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

#endregion

builder.Services.AddScoped<ICinemaContentService, CinemaContentService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<User>(options =>
{
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;

    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredUniqueChars = 4;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole<int>>()
.AddRoleManager<RoleManager<IdentityRole<int>>>()
.AddUserStore<UserStore<User, IdentityRole<int>, AppDbContext, int>>()
.AddRoleStore<RoleStore<IdentityRole<int>,AppDbContext,int>>()
.AddSignInManager()
.AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<User,IdentityRole<int>>>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services
.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler("/error");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("BlazorCorsPolicy");

// Распознавание кук пользователя
app.UseAuthentication();

// Проверка прав доступа
app.UseAuthorization();

app.MapControllers();

var uploadsPath = Path.Combine(app.Environment.ContentRootPath, ConstStrings.DefaultFilmImageDirectoryPath);

if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = $"/{ConstStrings.DefaultFilmImageDirectoryPath}"
});

app.Run();