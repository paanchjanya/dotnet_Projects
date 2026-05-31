using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManagement.Client.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async ValueTask SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _js.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch
        {
            // Fail silently if storage is unavailable or JS isn't ready
        }
    }

    public async ValueTask<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(json))
                return default;
            
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async ValueTask RemoveItemAsync(string key)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Fail silently
        }
    }
}
