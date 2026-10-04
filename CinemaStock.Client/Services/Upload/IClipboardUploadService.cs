using Microsoft.AspNetCore.Components;

namespace CinemaStock.Client.Services.Upload;

public interface IClipboardUploadService : IAsyncDisposable
{
    Task InitializeAsync(ElementReference dropArea, Action<string, string> onFileReceived);
    Task OpenExplorerAsync(ElementReference inputElement);
    Task DisposeHandlerAsync();
}