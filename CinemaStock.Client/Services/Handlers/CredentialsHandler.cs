using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace CinemaStock.Client.Services.Handlers;

public class CredentialsHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}