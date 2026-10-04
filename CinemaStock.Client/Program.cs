using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CinemaStock.Client;
using CinemaStock.Client.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using CinemaStock.Client.Services.Upload;
using CinemaStock.Client.Validation;
using CinemaStock.Client.Services.Clients;
using CinemaStock.Client.Extensions;
using CinemaStock.Shared.ApiEndpointControllers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#region Инициализация настроек (бэкенд)

var backendSettings = builder.Configuration.GetSection(nameof(BackendSettings)).Get<BackendSettings>();

if (backendSettings == null || string.IsNullOrWhiteSpace(backendSettings.ApiBaseUrl))
    throw new InvalidOperationException("Критическая ошибка: Секция 'BackendSettings:ApiBaseUrl' не найдена в appsettings.json клиента!");

builder.Services.AddSingleton(backendSettings);

#endregion

#region Сервисы авторизации и валидации

// Регистрация кастомного провайдера состояния
builder.Services.AddScoped<JwtAuthenticationStateProvider>();

// Использовать кастомный провайдер как стандартный
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<JwtAuthenticationStateProvider>());

// Базовые сервисы авторизации
builder.Services.AddAuthorizationCore();
builder.Services.AddLocalization();

builder.Services.AddScoped<IAccountService, AccountClient>();
builder.Services.AddScoped<IImageValidator, ImageValidator>();
builder.Services.AddTransient<IClipboardUploadService, ClipboardUploadService>();


#endregion

#region Настройка сети и клиентов (HTTP)

// Регистрируем хендлер для кук в DI контейнер
builder.Services.AddTransient<CookieHandler>();

var baseAddress = backendSettings.ApiBaseUrl;
ImageExtensions.Settings = backendSettings;

builder.Services.AddScoped(sp =>
{
    var cookieHandler = sp.GetRequiredService<CookieHandler>();
    cookieHandler.InnerHandler = new HttpClientHandler();

    return new HttpClient(cookieHandler)
    {
        BaseAddress = new Uri(baseAddress)
    };
});

// Регистрация доменных клиентов со встроенным CookieHandler
// Каждый клиент изолирован и автоматически шлет куки
builder.Services.AddCinemaStockClient<AccountClient>(baseAddress);
builder.Services.AddCinemaStockClient<CinemaContentClient>(baseAddress);
builder.Services.AddCinemaStockClient<GenreClient>(baseAddress);
builder.Services.AddCinemaStockClient<TagClient>(baseAddress);

builder.Services.AddScoped<IAccountService>(sp=>sp.GetRequiredService<AccountClient>());

#endregion

#region Логирование и запуск

await builder.Build().RunAsync();

#endregion

// Кастомный перехватчик, который автоматически добавляет куки к каждому запросу
public class CookieHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Принудительно включаем куки для текущего запроса
        Microsoft.AspNetCore.Components.WebAssembly.Http.WebAssemblyHttpRequestMessageExtensions
            .SetBrowserRequestCredentials(request, Microsoft.AspNetCore.Components.WebAssembly.Http.BrowserRequestCredentials.Include);

        return base.SendAsync(request, cancellationToken);
    }
}