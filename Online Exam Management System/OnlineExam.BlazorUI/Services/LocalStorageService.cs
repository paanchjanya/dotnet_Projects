using System.Text.Json;
using Microsoft.JSInterop;

namespace OnlineExam.BlazorUI.Services;

public sealed class LocalStorageService(IJSRuntime js)
{
    public ValueTask SetAsync<T>(string key, T value) =>
        js.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));

    public async ValueTask<T?> GetAsync<T>(string key)
    {
        var value = await js.InvokeAsync<string?>("localStorage.getItem", key);
        return string.IsNullOrWhiteSpace(value) ? default : JsonSerializer.Deserialize<T>(value);
    }

    public ValueTask RemoveAsync(string key) => js.InvokeVoidAsync("localStorage.removeItem", key);
}
