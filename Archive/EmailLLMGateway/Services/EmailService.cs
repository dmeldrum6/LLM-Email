using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using EmailLLMGateway.Models;

namespace EmailLLMGateway.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly HashSet<string> _processedMessageIds = new HashSet<string>();

        public EmailService(EmailSettings settings)
        {
            _settings = settings;
        }

        public async Task<List<EmailMessage>> CheckForNewEmailsAsync(string subjectPattern)
        {
            var newMessages = new List<EmailMessage>();

            try
            {
                using var client = new ImapClient();
                await client.ConnectAsync(_settings.ImapServer, _settings.ImapPort, _settings.ImapUseSsl);
                await client.AuthenticateAsync(_settings.EmailAddress, _settings.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);

                // Search for unread messages
                var uids = await inbox.SearchAsync(SearchQuery.NotSeen);

                foreach (var uid in uids)
                {
                    var message = await inbox.GetMessageAsync(uid);
                    string messageId = message.MessageId ?? $"{uid}_{DateTime.Now.Ticks}";

                    // Check if we've already processed this message
                    if (_processedMessageIds.Contains(messageId))
                        continue;

                    // Check if subject contains the pattern
                    if (message.Subject != null && message.Subject.Contains(subjectPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        newMessages.Add(new EmailMessage
                        {
                            MessageId = messageId,
                            Uid = uid,
                            Subject = message.Subject,
                            From = message.From.Mailboxes.FirstOrDefault()?.Address ?? "",
                            Body = message.TextBody ?? message.HtmlBody ?? "",
                            ReceivedDate = message.Date.DateTime
                        });

                        _processedMessageIds.Add(messageId);
                    }

                    // Mark as read
                    await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true);
                }

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking emails: {ex.Message}", ex);
            }

            return newMessages;
        }

        public async Task SendReplyAsync(string toAddress, string subject, string body, string inReplyTo = "")
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("LLM Email Gateway", _settings.EmailAddress));
                message.To.Add(MailboxAddress.Parse(toAddress));
                message.Subject = $"Re: {subject}";

                if (!string.IsNullOrEmpty(inReplyTo))
                {
                    message.InReplyTo = inReplyTo;
                }

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, _settings.SmtpUseSsl);

                // Use SMTP credentials if provided, otherwise use IMAP credentials
                string smtpUser = string.IsNullOrEmpty(_settings.SmtpUsername)
                    ? _settings.EmailAddress
                    : _settings.SmtpUsername;
                string smtpPass = string.IsNullOrEmpty(_settings.SmtpPassword)
                    ? _settings.Password
                    : _settings.SmtpPassword;

                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending email: {ex.Message}", ex);
            }
        }

        public void ClearProcessedMessages()
        {
            _processedMessageIds.Clear();
        }
    }

    public class EmailMessage
    {
        public string MessageId { get; set; } = "";
        public UniqueId Uid { get; set; }
        public string Subject { get; set; } = "";
        public string From { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime ReceivedDate { get; set; }
    }
}
