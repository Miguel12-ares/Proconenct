using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using ProConnect.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace ProConnect.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _password;
        private readonly string _frontendUrl;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _smtpServer = config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _port = int.TryParse(config["EmailSettings:Port"], out var port) ? port : 587;
            _senderEmail = config["EmailSettings:SenderEmail"] ?? "";
            _senderName = config["EmailSettings:SenderName"] ?? "ProConnect";
            _password = config["EmailSettings:Password"] ?? "";
            _frontendUrl = config["EmailSettings:FrontendUrl"] ?? "http://localhost:5089";
            _logger = logger;
        }

        public async Task SendVerificationEmailAsync(string to, string token)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_senderName, _senderEmail));
                message.To.Add(new MailboxAddress(to, to));
                message.Subject = "Verifica tu correo en ProConnect";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <h2>Bienvenido a ProConnect</h2>
                        <p>Gracias por registrarte. Por favor, verifica tu correo haciendo clic en el siguiente botón:</p>
                        <a href='{_frontendUrl}/api/auth/verify-email/{token}' style='display:inline-block;padding:10px 20px;background:#007bff;color:#fff;text-decoration:none;border-radius:5px;'>Verificar mi correo</a>
                        <p>Si no puedes hacer clic, copia y pega este enlace en tu navegador:</p>
                        <p>{_frontendUrl}/api/auth/verify-email/{token}</p>
                        <br><p>El equipo de ProConnect</p>"
                };
                message.Body = bodyBuilder.ToMessageBody();

                _logger.LogInformation($"Intentando enviar correo a {to} usando {_smtpServer}:{_port} como {_senderEmail}");

                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpServer, _port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_senderEmail, _password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Correo de verificación enviado exitosamente a {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de verificación a {to}");
                throw;
            }
        }
    }
} 