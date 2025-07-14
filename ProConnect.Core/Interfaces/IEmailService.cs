namespace ProConnect.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string to, string token);
    }
} 