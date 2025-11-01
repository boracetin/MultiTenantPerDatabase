using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure.Services.Implementations;

/// <summary>
/// Email service implementation
/// Infrastructure layer'da external dependency (SMTP, SendGrid, etc.)
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Production'da: SMTP, SendGrid, AWS SES, etc.
        _logger.LogInformation("Sending email to {To} - Subject: {Subject}", to, subject);
        
        // Demo için sadece log
        await Task.Delay(100); // Simulate external call
        
        _logger.LogInformation("Email sent successfully to {To}", to);
    }

    public async Task SendProductCreatedNotificationAsync(int productId, string productName)
    {
        var adminEmail = _configuration["AdminEmail"] ?? "admin@example.com";
        
        var subject = "New Product Created";
        var body = $@"
            <h2>New Product Added</h2>
            <p><strong>Product ID:</strong> {productId}</p>
            <p><strong>Product Name:</strong> {productName}</p>
            <p>This is an automated notification.</p>
        ";

        await SendEmailAsync(adminEmail, subject, body);
    }
}

/// <summary>
/// Fake implementation for testing
/// </summary>
public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger;

    public FakeEmailService(ILogger<FakeEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("[FAKE EMAIL] To: {To}, Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendProductCreatedNotificationAsync(int productId, string productName)
    {
        _logger.LogInformation("[FAKE EMAIL] Product created notification: {ProductId} - {ProductName}", 
            productId, productName);
        return Task.CompletedTask;
    }
}
