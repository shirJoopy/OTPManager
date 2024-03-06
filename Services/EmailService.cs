using MailKit.Net.Smtp;
using MimeKit;
using System.Net;
using System.Net.Mail;
using System.Reflection.PortableExecutable;
using Microsoft.Extensions.Options;
using OTPManager.Models;
using MailKit.Security;
using OTPManager.Services.Interfaces;

namespace OTPManager.Services
{
   


    public class EmailService : IEMailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }


        public async Task SendEmailAsync(string toAddress, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.From, _emailSettings.SmtpUsername)); // Your email address
            message.To.Add(new MailboxAddress("", toAddress)); // Recipient's email address
            message.Subject = subject;

            message.Body = new TextPart("html") // or "plain" if plain text
            {
                Text = body
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                // For SSL connections
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls); // Use your SMTP server's settings
                                                                          // For non-SSL connections
                                                                          // await client.ConnectAsync("smtp.example.com", 587, false); // Use your SMTP server's settings

                // Note: only needed if SMTP server requires authentication
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }


    }
}
