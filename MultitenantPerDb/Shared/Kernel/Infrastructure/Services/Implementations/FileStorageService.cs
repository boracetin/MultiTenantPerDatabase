using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure.Services.Implementations;

/// <summary>
/// File storage service implementation
/// Infrastructure layer - cloud storage (Azure Blob, AWS S3, etc.)
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _storageBasePath;

    public FileStorageService(ILogger<FileStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _storageBasePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        
        // Create directory if not exists
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string? contentType = null)
    {
        try
        {
            // Production: Azure Blob Storage, AWS S3, etc.
            _logger.LogInformation("Uploading file: {FileName}, ContentType: {ContentType}", 
                fileName, contentType ?? "unknown");

            // Generate unique file name
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_storageBasePath, uniqueFileName);

            // Save file
            using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamOutput);

            _logger.LogInformation("File uploaded successfully: {FileName} -> {FilePath}", fileName, filePath);

            // Return file URL (in production, return cloud storage URL)
            return $"/uploads/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File upload failed: {FileName}", fileName);
            throw;
        }
    }

    public Task<string> GetFileUrlAsync(string fileName)
    {
        // Production: Return full cloud storage URL
        var url = $"/uploads/{fileName}";
        return Task.FromResult(url);
    }

    public async Task<Stream> DownloadFileAsync(string fileUrl)
    {
        try
        {
            // Extract file name from URL
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_storageBasePath, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {fileUrl}");
            }

            _logger.LogInformation("Downloading file: {FileUrl}", fileUrl);

            var memoryStream = new MemoryStream();
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File download failed: {FileUrl}", fileUrl);
            throw;
        }
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_storageBasePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FileUrl}", fileUrl);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File deletion failed: {FileUrl}", fileUrl);
            throw;
        }
    }

    public Task<bool> FileExistsAsync(string fileUrl)
    {
        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_storageBasePath, fileName);
        return Task.FromResult(File.Exists(filePath));
    }
}

/// <summary>
/// Fake file storage for testing
/// </summary>
public class FakeFileStorageService : IFileStorageService
{
    private readonly ILogger<FakeFileStorageService> _logger;
    private readonly Dictionary<string, byte[]> _inMemoryStorage = new();

    public FakeFileStorageService(ILogger<FakeFileStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string? contentType = null)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        _inMemoryStorage[uniqueFileName] = memoryStream.ToArray();

        _logger.LogInformation("[FAKE STORAGE] File uploaded: {FileName}, ContentType: {ContentType}", 
            uniqueFileName, contentType ?? "unknown");
        
        return $"/fake-storage/{uniqueFileName}";
    }

    public Task<string> GetFileUrlAsync(string fileName)
    {
        var url = $"/fake-storage/{fileName}";
        return Task.FromResult(url);
    }

    public Task<Stream> DownloadFileAsync(string fileUrl)
    {
        var fileName = Path.GetFileName(fileUrl);
        
        if (!_inMemoryStorage.ContainsKey(fileName))
        {
            throw new FileNotFoundException($"File not found: {fileUrl}");
        }

        _logger.LogInformation("[FAKE STORAGE] File downloaded: {FileName}", fileName);
        
        return Task.FromResult<Stream>(new MemoryStream(_inMemoryStorage[fileName]));
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        var fileName = Path.GetFileName(fileUrl);
        
        if (_inMemoryStorage.ContainsKey(fileName))
        {
            _inMemoryStorage.Remove(fileName);
            _logger.LogInformation("[FAKE STORAGE] File deleted: {FileName}", fileName);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string fileUrl)
    {
        var fileName = Path.GetFileName(fileUrl);
        return Task.FromResult(_inMemoryStorage.ContainsKey(fileName));
    }
}
