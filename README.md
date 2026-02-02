# Email LLM Gateway

A Windows desktop application built with C# and .NET 8 that monitors an email inbox and automatically processes queries through a Large Language Model (LLM), sending intelligent responses back to users via email.

## Features

- **Email Monitoring**: Continuously monitors a configured email inbox using IMAP
- **LLM Integration**: Supports Claude API (Anthropic) for intelligent query processing
- **Automatic Replies**: Sends email responses automatically via SMTP
- **Subject-Based Triggering**: Only processes emails with a specific pattern in the subject line
- **System Tray Integration**: Minimize to system tray for unobtrusive background operation
- **Configuration UI**: User-friendly interface for managing all settings
- **Secure Storage**: Passwords and API keys are encrypted using Windows DPAPI
- **Logging**: Comprehensive logging system for monitoring and troubleshooting

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime
- Visual Studio 2022 (for development)
- Email account with IMAP/SMTP access
- Claude API key from Anthropic

## Installation

### For Users

1. Download the latest release from the releases page
2. Extract the ZIP file to your desired location
3. Run `EmailLLMGateway.exe`

### For Developers

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/LLM-Email.git
   ```

2. Open `EmailLLMGateway.sln` in Visual Studio 2022

3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

4. Build the solution:
   ```bash
   dotnet build
   ```

5. Run the application:
   ```bash
   dotnet run --project EmailLLMGateway
   ```

## Configuration

### Email Settings

1. Navigate to the **Email Settings** tab
2. Configure IMAP settings:
   - **IMAP Server**: Your email provider's IMAP server (e.g., `imap.gmail.com`)
   - **IMAP Port**: Usually 993 for SSL
   - **Email Address**: Your email address
   - **Password**: Your email password or app-specific password
   - **Use SSL**: Keep checked for secure connection

3. Configure SMTP settings:
   - **SMTP Server**: Your email provider's SMTP server (e.g., `smtp.gmail.com`)
   - **SMTP Port**: Usually 587 for TLS or 465 for SSL
   - **SMTP Username**: (Optional) If different from email address
   - **SMTP Password**: (Optional) If different from email password
   - **Use SSL**: Keep checked for secure connection

4. Click **Save Settings**

#### Gmail Setup

For Gmail users:
1. Enable IMAP in Gmail settings
2. Generate an App Password:
   - Go to Google Account settings
   - Navigate to Security > 2-Step Verification > App passwords
   - Generate a new app password
   - Use this password in the application

### LLM Settings

1. Navigate to the **LLM Settings** tab
2. Configure:
   - **Provider**: Select "Anthropic" (currently the only supported provider)
   - **API Key**: Your Claude API key from [Anthropic Console](https://console.anthropic.com/)
   - **Model**: Model name (e.g., `claude-3-5-sonnet-20241022`)
   - **Max Tokens**: Maximum response length (default: 4096)

3. Click **Save Settings**

### Monitoring Settings

1. Navigate to the **Monitoring** tab
2. Configure:
   - **Subject Pattern**: Text that must appear in email subject to trigger processing (e.g., `[LLM]`)
   - **Polling Interval**: How often to check for new emails (in seconds)
   - **Auto-start monitoring**: Automatically start monitoring when application launches
   - **Minimize to system tray**: Minimize to tray instead of taskbar

3. Click **Save Settings**

## Usage

### Starting the Service

1. Ensure all settings are configured correctly
2. Go to the **Monitoring** tab
3. Click **Start Monitoring**
4. The service will now check for new emails at the configured interval

### Sending Queries

1. Send an email to the configured email address
2. Include the configured subject pattern in the subject line (e.g., `[LLM] What is the capital of France?`)
3. Write your query in the email body
4. The application will:
   - Detect the email
   - Extract the query
   - Send it to the LLM
   - Reply with the LLM's response

### Example Email

**Subject**: `[LLM] Help with Python code`

**Body**:
```
Can you help me write a Python function to calculate the Fibonacci sequence?
I need it to handle large numbers efficiently.
```

**Response**: You'll receive an automated reply with the LLM's response.

### Monitoring

- View real-time logs in the **Log** tab
- Monitor the status in the **Monitoring** tab
- The application logs all activities to files in `%APPDATA%\EmailLLMGateway\logs\`

### System Tray

- When minimized to the system tray, the application continues running in the background
- Right-click the tray icon to:
  - Show the main window
  - Exit the application
- Double-click to restore the window

## Security Considerations

- Passwords and API keys are encrypted using Windows Data Protection API (DPAPI)
- Configuration files are stored in `%APPDATA%\EmailLLMGateway\`
- Only the current Windows user can decrypt the sensitive data
- Use app-specific passwords when possible (especially for Gmail)
- Keep your API keys confidential

## Troubleshooting

### Email Connection Issues

- Verify your email credentials
- Check that IMAP/SMTP are enabled in your email provider
- For Gmail, ensure you're using an App Password
- Check firewall settings
- Verify server addresses and ports

### LLM API Issues

- Verify your API key is correct
- Check your Anthropic account has available credits
- Ensure the model name is correct
- Check internet connectivity

### Application Not Processing Emails

- Verify the subject pattern matches your emails
- Check the Log tab for error messages
- Ensure monitoring service is started
- Verify email is marked as unread

## Architecture

### Project Structure

```
EmailLLMGateway/
├── Models/
│   └── AppConfiguration.cs       # Configuration data models
├── Services/
│   ├── ConfigurationManager.cs   # Config save/load with encryption
│   ├── EmailService.cs            # IMAP/SMTP email handling
│   ├── LLMService.cs              # LLM API integration
│   ├── LoggingService.cs          # Logging functionality
│   └── MonitoringService.cs       # Main orchestration service
├── MainForm.cs                    # Main UI form
└── Program.cs                     # Application entry point
```

### Key Dependencies

- **MailKit**: Email handling (IMAP/SMTP)
- **MimeKit**: Email message parsing
- **Anthropic.SDK**: Claude API integration
- **Newtonsoft.Json**: Configuration serialization

## Future Enhancements

Potential features for future versions:
- Support for additional LLM providers (OpenAI, Google, etc.)
- Email templates for responses
- Multiple email account monitoring
- Advanced filtering rules
- Response caching
- Rate limiting
- Web interface
- Docker support for cross-platform deployment

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or suggestions, please open an issue on GitHub.

## Acknowledgments

- Built with [MailKit](https://github.com/jstedfast/MailKit) by Jeffrey Stedfast
- LLM integration powered by [Anthropic's Claude](https://www.anthropic.com/)
- Uses [Anthropic SDK for .NET](https://github.com/tghamm/Anthropic.SDK)
