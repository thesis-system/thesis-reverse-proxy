namespace Thesis.ReverseProxy;

/// <summary>
/// Настройки сервисов
/// </summary>
public class ThesisServices
{
    /// <summary>
    /// Публичный урл
    /// </summary>
    public string PublicUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Урл сервиса авторизации
    /// </summary>
    public string AuthUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Урл сервиса изображений
    /// </summary>
    public string ImagesUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Урл сервиса заявок
    /// </summary>
    public string RequestsUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Урл сервиса активов
    /// </summary>
    public string AssetsUrl { get; set; } = string.Empty;
}
