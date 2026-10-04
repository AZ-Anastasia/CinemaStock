using System.Globalization;
using System.Resources;

namespace CinemaStock.Shared.Resources;

/// <summary>
/// Базовый класс для UserErrors, чтобы динамически получать язык системы/браузера для отображения ошибки
/// Нужно для использования в .cs файлах
/// </summary>
public abstract class BaseResource
{
    private static readonly Lazy<ResourceManager> _resourceManager = new(() =>
        new ResourceManager(typeof(UserErrors.UserErrors).FullName!, typeof(UserErrors.UserErrors).Assembly));

    protected static string GetValue(string key)
    {
        return _resourceManager.Value.GetString(key, CultureInfo.CurrentUICulture) ?? $"[{key}]";
    }
}