using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;
using System.Text;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure.Services.Implementations;

/// <summary>
/// Generic Email Service Implementation
/// Pure infrastructure - no domain knowledge
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
        // Production: SMTP, SendGrid, AWS SES, Mailgun, etc.
        _logger.LogInformation("Sending email to {To} - Subject: {Subject}", to, subject);
        
        try
        {
            // Demo - simulate external SMTP call
            await Task.Delay(100);
            
            // Production code example:
            // using var client = new SmtpClient();
            // client.Connect(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]));
            // client.Authenticate(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
            // await client.SendAsync(message);
            
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendTemplatedEmailAsync(string to, string subject, string templateName, Dictionary<string, string> templateData)
    {
        // Load template and replace placeholders
        var templateBody = LoadEmailTemplate(templateName);
        
        foreach (var (key, value) in templateData)
        {
            templateBody = templateBody.Replace($"{{{key}}}", value);
        }

        await SendEmailAsync(to, subject, templateBody);
    }

    public async Task SendEmailWithAttachmentsAsync(string to, string subject, string body, List<EmailAttachment> attachments)
    {
        _logger.LogInformation("Sending email with {AttachmentCount} attachments to {To}", 
            attachments.Count, to);
        
        try
        {
            // Production: Build email message with attachments
            await Task.Delay(100);
            
            _logger.LogInformation("Email with attachments sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with attachments to {To}", to);
            throw;
        }
    }

    private string LoadEmailTemplate(string templateName)
    {
        // Production: Load from file system or database
        var templatePath = Path.Combine("EmailTemplates", $"{templateName}.html");
        
        if (File.Exists(templatePath))
        {
            return File.ReadAllText(templatePath);
        }

        // Default template
        return @"
            <html>
                <body>
                    <div style='font-family: Arial, sans-serif;'>
                        {{Content}}
                    </div>
                </body>
            </html>
        ";
    }
}

/// <summary>
/// Fake Email Service for Development/Testing
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
        _logger.LogInformation("[FAKE EMAIL] To: {To}, Subject: {Subject}, Body Length: {BodyLength}", 
            to, subject, body.Length);
        return Task.CompletedTask;
    }

    public Task SendTemplatedEmailAsync(string to, string subject, string templateName, Dictionary<string, string> templateData)
    {
        _logger.LogInformation("[FAKE EMAIL] Templated - To: {To}, Template: {Template}, Data: {@Data}", 
            to, templateName, templateData);
        return Task.CompletedTask;
    }

    public Task SendEmailWithAttachmentsAsync(string to, string subject, string body, List<EmailAttachment> attachments)
    {
        _logger.LogInformation("[FAKE EMAIL] With Attachments - To: {To}, Attachments: {Count}", 
            to, attachments.Count);
        return Task.CompletedTask;
    }
}
