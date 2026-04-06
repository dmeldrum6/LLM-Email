using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailLLMResponder.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;

namespace EmailLLMResponder.Services
{
    public class EmailService
    {
        private readonly HashSet<string> _processedMessageIds = new HashSet<string>();

        public async Task<List<EmailMessage>> GetUnreadEmailsAsync(EmailConfig config)
        {
            var emails = new List<EmailMessage>();

            try
            {
                using var client = new ImapClient();
                await client.ConnectAsync(config.ImapServer, config.ImapPort, config.ImapUseSsl);
                await client.AuthenticateAsync(config.EmailAddress, config.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);

                var uids = await inbox.SearchAsync(SearchQuery.NotSeen);

                foreach (var uid in uids)
                {
                    var message = await inbox.GetMessageAsync(uid);
                    var messageId = message.MessageId ?? uid.ToString();

                    if (!_processedMessageIds.Contains(messageId))
                    {
                        emails.Add(new EmailMessage
                        {
                            MessageId = messageId,
                            Uid = uid,
                            From = message.From.Mailboxes.FirstOrDefault()?.Address ?? "unknown",
                            FromName = message.From.Mailboxes.FirstOrDefault()?.Name ?? "Unknown",
                            Subject = message.Subject ?? "(No Subject)",
                            Body = message.TextBody ?? message.HtmlBody ?? "(No Body)",
                            ReceivedDate = message.Date.DateTime
                        });

                        _processedMessageIds.Add(messageId);
                    }
                }

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving emails: {ex.Message}", ex);
            }

            return emails;
        }

        public async Task SendEmailAsync(string to, string subject, string body, EmailConfig config)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(config.EmailAddress, config.EmailAddress));
                message.To.Add(new MailboxAddress(to, to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(config.SmtpServer, config.SmtpPort, GetSmtpOptions(config));
                await client.AuthenticateAsync(config.EmailAddress, config.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending email: {ex.Message}", ex);
            }
        }

        public async Task MarkAsReadAsync(UniqueId uid, EmailConfig config)
        {
            try
            {
                using var client = new ImapClient();
                await client.ConnectAsync(config.ImapServer, config.ImapPort, config.ImapUseSsl);
                await client.AuthenticateAsync(config.EmailAddress, config.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);
                await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true);

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error marking email as read: {ex.Message}", ex);
            }
        }

        public async Task TestConnectionAsync(EmailConfig config)
        {
            using var imapClient = new ImapClient();
            await imapClient.ConnectAsync(config.ImapServer, config.ImapPort, config.ImapUseSsl);
            await imapClient.AuthenticateAsync(config.EmailAddress, config.Password);
            await imapClient.DisconnectAsync(true);

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(config.SmtpServer, config.SmtpPort, GetSmtpOptions(config));
            await smtpClient.AuthenticateAsync(config.EmailAddress, config.Password);
            await smtpClient.DisconnectAsync(true);
        }

        private static SecureSocketOptions GetSmtpOptions(EmailConfig config)
        {
            if (!config.SmtpUseSsl)
                return SecureSocketOptions.StartTlsWhenAvailable;
            // Port 465 uses implicit TLS (SSL on connect); port 587 uses explicit TLS (STARTTLS)
            return config.SmtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;
        }
    }

    public class EmailMessage
    {
        public string MessageId { get; set; } = string.Empty;
        public UniqueId Uid { get; set; }
        public string From { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; }
    }
}
