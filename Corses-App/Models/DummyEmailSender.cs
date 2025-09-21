using Microsoft.AspNetCore.Identity.UI.Services;

namespace Corses_App.Models
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // فقط للعرض، لا ترسل شيء
            Console.WriteLine($"[Email] To: {email}, Subject: {subject}");
            return Task.CompletedTask;
        }
    }
}
