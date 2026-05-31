using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace TaskManagement.Client.Services;

public class ThemeManager
{
    private readonly LocalStorageService _localStorage;
    private readonly IJSRuntime _js;

    public string CurrentTheme { get; private set; } = "midnight";

    public event Action? OnThemeChanged;

    public ThemeManager(LocalStorageService localStorage, IJSRuntime js)
    {
        _localStorage = localStorage;
        _js = js;
    }

    public async Task InitializeThemeAsync()
    {
        var savedTheme = await _localStorage.GetItemAsync<string>("userTheme");
        if (!string.IsNullOrEmpty(savedTheme))
        {
            CurrentTheme = savedTheme;
        }
        await ApplyThemeAsync(CurrentTheme);
    }

    public async Task ChangeThemeAsync(string themeName)
    {
        CurrentTheme = themeName;
        await _localStorage.SetItemAsync("userTheme", themeName);
        await ApplyThemeAsync(themeName);
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyThemeAsync(string themeName)
    {
        try
        {
            await _js.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", themeName);
        }
        catch
        {
            // Fail silent during initial render if JS runtime isn't fully ready
        }
    }
}
