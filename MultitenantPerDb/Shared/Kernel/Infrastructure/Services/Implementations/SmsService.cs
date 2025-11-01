using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;
using System.Text.RegularExpressions;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure.Services.Implementations;

/// <summary>
/// Generic SMS Service Implementation
/// Pure infrastructure - no domain knowledge
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;

    public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // Validate phone number format
        if (!IsValidPhoneNumber(phoneNumber))
        {
            _logger.LogWarning("Invalid phone number format: {PhoneNumber}", phoneNumber);
            throw new ArgumentException("Invalid phone number format", nameof(phoneNumber));
        }

        // Production: Twilio, AWS SNS, Nexmo, MessageBird, etc.
        _logger.LogInformation("Sending SMS to {PhoneNumber} - Length: {Length}", 
            MaskPhoneNumber(phoneNumber), message.Length);
        
        try
        {
            // Demo - simulate external SMS API call
            await Task.Delay(100);
            
            // Production code example (Twilio):
            // var client = new TwilioRestClient(accountSid, authToken);
            // await MessageResource.CreateAsync(
            //     to: new PhoneNumber(phoneNumber),
            //     from: new PhoneNumber(_configuration["Twilio:FromNumber"]),
            //     body: message
            // );
            
            _logger.LogInformation("SMS sent successfully to {PhoneNumber}", MaskPhoneNumber(phoneNumber));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            throw;
        }
    }

    public async Task SendTemplatedSmsAsync(string phoneNumber, string templateKey, Dictionary<string, string> templateData)
    {
        // Load template and replace placeholders
        var template = LoadSmsTemplate(templateKey);
        
        var message = template;
        foreach (var (key, value) in templateData)
        {
            message = message.Replace($"{{{key}}}", value);
        }

        // SMS length validation (160 chars for single SMS)
        if (message.Length > 160)
        {
            _logger.LogWarning("SMS message exceeds 160 characters ({Length}). Will be split into multiple parts.", 
                message.Length);
        }

        await SendSmsAsync(phoneNumber, message);
    }

    private bool IsValidPhoneNumber(string phoneNumber)
    {
        // Basic validation - can be extended for international formats
        // E.164 format: +[country code][number]
        var pattern = @"^\+?[1-9]\d{1,14}$";
        return Regex.IsMatch(phoneNumber, pattern);
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        // Mask for privacy in logs: +90 555 123 45 67 -> +90 555 *** ** 67
        if (phoneNumber.Length < 8) return phoneNumber;
        
        var visible = phoneNumber.Length > 10 ? 4 : 2;
        var start = phoneNumber[..visible];
        var end = phoneNumber[^visible..];
        var masked = new string('*', phoneNumber.Length - (visible * 2));
        
        return $"{start}{masked}{end}";
    }

    private string LoadSmsTemplate(string templateKey)
    {
        // Production: Load from configuration or database
        var templates = _configuration.GetSection("SmsTemplates").Get<Dictionary<string, string>>();
        
        if (templates != null && templates.TryGetValue(templateKey, out var template))
        {
            return template;
        }

        // Default template
        return "{Message}";
    }
}

/// <summary>
/// Fake SMS Service for Development/Testing
/// </summary>
public class FakeSmsService : ISmsService
{
    private readonly ILogger<FakeSmsService> _logger;

    public FakeSmsService(ILogger<FakeSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("[FAKE SMS] To: {PhoneNumber}, Message: {Message}", 
            phoneNumber, message);
        return Task.CompletedTask;
    }

    public Task SendTemplatedSmsAsync(string phoneNumber, string templateKey, Dictionary<string, string> templateData)
    {
        _logger.LogInformation("[FAKE SMS] Templated - To: {PhoneNumber}, Template: {Template}, Data: {@Data}", 
            phoneNumber, templateKey, templateData);
        return Task.CompletedTask;
    }
}
