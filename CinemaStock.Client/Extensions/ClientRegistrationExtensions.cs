using CinemaStock.Client.Services.Clients.Base;
using CinemaStock.Client.Services.Handlers;

namespace CinemaStock.Client.Extensions;

public static class ClientRegistrationExtensions
{
    public static IServiceCollection AddCinemaStockClient<TClient>(
        this IServiceCollection services,
        string baseAddress) where TClient : ApiHttpClient
    {
        return services.AddScoped(sp =>
        {
            var credentialsHandler = new CredentialsHandler();
            credentialsHandler.InnerHandler = new HttpClientHandler();

            var http = new HttpClient(credentialsHandler)
            {
                BaseAddress = new Uri(baseAddress)
            };

            // Передача настроенного http в конструктор клиента
            return (TClient)Activator.CreateInstance(typeof(TClient), http)!;
        });
    }
}