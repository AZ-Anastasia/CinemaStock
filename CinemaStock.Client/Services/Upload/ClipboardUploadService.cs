using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CinemaStock.Client.Services.Upload;

public class ClipboardUploadService : IClipboardUploadService
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _jsHandlerInstance;
    private DotNetObjectReference<ClipboardBridge>? _dotNetRef;
    private ILogger<ClipboardUploadService> _logger;

    public ClipboardUploadService(IJSRuntime jSRuntime, ILogger<ClipboardUploadService> logger)
    {
        _jsRuntime = jSRuntime;
        _logger = logger;
    }

    public async Task InitializeAsync(ElementReference dropArea, Action<string, string> onFileReceived)
    {
        if (_jsHandlerInstance != null) return;

        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/CmsDragAndDrop.js");

        var bridge = new ClipboardBridge(onFileReceived);
        _dotNetRef = DotNetObjectReference.Create(bridge);

        _jsHandlerInstance = await _jsModule.InvokeAsync<IJSObjectReference>("initialize", dropArea, _dotNetRef);
    }

    public async Task OpenExplorerAsync(ElementReference inputElement)
    {
        if (_jsModule != null && inputElement.Context != null)
            try
            {
            await _jsModule.InvokeVoidAsync("openExplorer", inputElement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось программно открыть проводник");
            }
    }

    public async Task DisposeHandlerAsync()
    {
        if (_jsHandlerInstance != null)
        {
            await _jsHandlerInstance.InvokeVoidAsync("dispose");
            await _jsHandlerInstance.DisposeAsync();
            _jsHandlerInstance = null;
        }

        _dotNetRef?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeHandlerAsync();
        if (_jsModule != null) await _jsModule.DisposeAsync();
    }
}

public class ClipboardBridge
{
    private readonly Action<string, string> _onFileReceived;

    public ClipboardBridge(Action<string, string> onFileReceived)
    {
        _onFileReceived = onFileReceived;
    }

    [JSInvokable]
    public void HandleExternalFile(string base64Data, string fileName)
    {
        _onFileReceived?.Invoke(base64Data, fileName);
    }
}