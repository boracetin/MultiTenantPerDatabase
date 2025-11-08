namespace MultitenantPerDb.Core.Infrastructure.Services;

/// <summary>
/// Infrastructure Service - Generic Email Service
/// Pure infrastructure concern - no domain knowledge
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email with subject and body
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body);
    
    /// <summary>
    /// Send email with HTML template
    /// </summary>
    Task SendTemplatedEmailAsync(string to, string subject, string templateName, Dictionary<string, string> templateData);
    
    /// <summary>
    /// Send email with attachments
    /// </summary>
    Task SendEmailWithAttachmentsAsync(string to, string subject, string body, List<EmailAttachment> attachments);
}

/// <summary>
/// Email attachment model
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}

/// <summary>
/// Infrastructure Service - Generic SMS Service
/// Pure infrastructure concern - no domain knowledge
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send SMS message to phone number
    /// </summary>
    Task SendSmsAsync(string phoneNumber, string message);
    
    /// <summary>
    /// Send SMS with template
    /// </summary>
    Task SendTemplatedSmsAsync(string phoneNumber, string templateName, Dictionary<string, string> templateData);
}

/// <summary>
/// Infrastructure Service - Generic File Storage
/// Pure infrastructure concern - no domain knowledge
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string? contentType = null);
    Task<Stream> DownloadFileAsync(string fileUrl);
    Task DeleteFileAsync(string fileUrl);
    Task<bool> FileExistsAsync(string fileUrl);
    Task<string> GetFileUrlAsync(string fileName);
}

/// <summary>
/// Infrastructure Service - Generic HTTP Client Service
/// For external API calls - pure infrastructure
/// </summary>
public interface IHttpClientService
{
    Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data, Dictionary<string, string>? headers = null);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data, Dictionary<string, string>? headers = null);
    Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null);
}
