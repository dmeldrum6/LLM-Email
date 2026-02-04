# Email LLM Responder

A WPF desktop application that automatically monitors an email inbox, processes incoming questions using an OpenAI API-compatible LLM, and sends intelligent responses back to the sender.

## Features

- **Automated Email Monitoring**: Continuously checks an email inbox for new messages via IMAP
- **LLM-Powered Responses**: Generates intelligent responses using any OpenAI API-compatible LLM service
- **Automatic Reply**: Sends generated responses back to the original sender via SMTP
- **User-Friendly Interface**: Clean tabbed interface for easy configuration and monitoring
- **Secure Configuration**: Passwords and API keys are encrypted and stored securely
- **Real-Time Logging**: Monitor all activity in real-time with detailed logging

## Requirements

- Windows operating system
- .NET 8.0 or later
- Email account with IMAP/SMTP access
- OpenAI API-compatible LLM service (OpenAI, Azure OpenAI, local LLM server, etc.)

## Building the Application

1. Open the solution in Visual Studio 2022 or later
2. Restore NuGet packages
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

Alternatively, build from command line:
```bash
cd EmailLLMResponder
dotnet restore
dotnet build
dotnet run
```

## Configuration

### Email Configuration Tab

Configure your email server settings:

**IMAP Settings (Incoming Mail):**
- **IMAP Server**: Your email provider's IMAP server (e.g., `imap.gmail.com`)
- **IMAP Port**: Usually 993 for SSL/TLS
- **Use SSL/TLS**: Keep checked for secure connection

**SMTP Settings (Outgoing Mail):**
- **SMTP Server**: Your email provider's SMTP server (e.g., `smtp.gmail.com`)
- **SMTP Port**: Usually 587 for TLS or 465 for SSL
- **Use SSL/TLS**: Keep checked for secure connection

**Authentication:**
- **Email Address**: Your full email address
- **Password**: Your email password or app-specific password

**Monitoring Settings:**
- **Check Interval**: How often to check for new emails (in seconds)

**Common Email Providers:**

*Gmail:*
- IMAP: `imap.gmail.com:993`
- SMTP: `smtp.gmail.com:587`
- Note: Enable "Less secure app access" or use an App Password

*Outlook/Office 365:*
- IMAP: `outlook.office365.com:993`
- SMTP: `smtp.office365.com:587`

*Yahoo:*
- IMAP: `imap.mail.yahoo.com:993`
- SMTP: `smtp.mail.yahoo.com:587`

### LLM Configuration Tab

Configure your LLM service:

**API Settings:**
- **API Endpoint**: The OpenAI-compatible API endpoint
  - OpenAI: `https://api.openai.com/v1/chat/completions`
  - Azure OpenAI: `https://YOUR-RESOURCE.openai.azure.com/openai/deployments/YOUR-DEPLOYMENT/chat/completions?api-version=2024-02-15-preview`
  - Local LLM (e.g., LM Studio): `http://localhost:1234/v1/chat/completions`
- **API Key**: Your API key for authentication
- **Model**: Model name (e.g., `gpt-3.5-turbo`, `gpt-4`, or your deployment name)

**Generation Parameters:**
- **Temperature**: Controls randomness (0.0 = deterministic, 2.0 = very random)
- **Max Tokens**: Maximum length of generated response

**System Prompt:**
- Customize how the LLM should respond to emails
- Default: "You are a helpful email assistant. Respond to emails in a professional and concise manner."

### Activity Log Tab

Monitor the application's operation:

- **Start Monitoring**: Begin checking emails and sending automated responses
- **Stop Monitoring**: Pause the monitoring service
- **Status Indicator**: Shows current operational status
- **Real-Time Log**: Displays all activities including:
  - Email checks
  - Emails received
  - LLM responses generated
  - Replies sent
  - Errors and warnings

## Usage Workflow

1. **Configure Email Settings**
   - Enter your email server details
   - Click "Test Connection" to verify settings
   - Click "Save Email Config" to store configuration

2. **Configure LLM Settings**
   - Enter your LLM API endpoint and key
   - Customize the system prompt if desired
   - Click "Test Connection" to verify LLM access
   - Click "Save LLM Config" to store configuration

3. **Start Monitoring**
   - Navigate to the Activity Log tab
   - Click "Start Monitoring"
   - The application will now:
     - Check for new emails at the configured interval
     - Process each unread email
     - Generate a response using the LLM
     - Send the response to the original sender
     - Mark the email as read

4. **Monitor Activity**
   - Watch the Activity Log for real-time updates
   - Check for any errors or issues
   - Use "Clear Log" to clean up the display

## Security Notes

- Passwords and API keys are encrypted using Windows Data Protection API (DPAPI)
- Configuration is stored in `%APPDATA%\EmailLLMResponder\config.json`
- Credentials are encrypted per-user and per-machine
- Always use SSL/TLS for email connections
- Consider using app-specific passwords for email accounts

## Troubleshooting

**Email Connection Failed:**
- Verify IMAP/SMTP server addresses and ports
- Check that SSL/TLS settings are correct
- Ensure your email provider allows IMAP/SMTP access
- For Gmail, enable 2-factor authentication and create an App Password

**LLM Connection Failed:**
- Verify API endpoint URL is correct
- Check that your API key is valid and has sufficient credits
- Ensure your network can reach the API endpoint
- For local LLMs, verify the server is running

**No Emails Being Processed:**
- Check the Activity Log for errors
- Verify the check interval is set appropriately
- Ensure there are unread emails in your inbox
- Check that the monitoring service is running (Status: Running)

**Responses Not Being Sent:**
- Verify SMTP settings are correct
- Check that your email account allows sending emails
- Review the Activity Log for sending errors

## Dependencies

- **MailKit**: Email IMAP/SMTP client library
- **MimeKit**: Email message parsing and creation
- **Newtonsoft.Json**: Configuration serialization

## License

See LICENSE file for details.

## Archive

The previous version of this application has been moved to the `/Archive` folder.
